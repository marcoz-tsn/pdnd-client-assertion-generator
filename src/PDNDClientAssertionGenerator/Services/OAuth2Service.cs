﻿// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Italia.Pdnd.Identity.Client.OAuth2;
using PDNDClientAssertionGenerator.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using Italia.Pdnd.Identity.Client.AppConfig;

namespace PDNDClientAssertionGenerator.Services
{
    /// <summary>
    /// Service for handling OAuth2 client assertion generation and token requests.
    /// </summary>
    public class OAuth2Service : IOAuth2Service
    {
        private readonly ClientAssertionConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2Service"/> class.
        /// </summary>
        /// <param name="config">An <see cref="IOptions{ClientAssertionConfig}"/> object containing the configuration for client assertion generation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
        public OAuth2Service(IOptions<ClientAssertionConfig> config)
        {
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Asynchronously generates a client assertion (JWT) token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the generated client assertion as a string.</returns>
        public async Task<string> GenerateClientAssertionAsync()
        {
            // Generate a unique token ID (JWT ID)
            var tokenId = Guid.NewGuid().ToString("D").ToLower();

            // Define the current UTC time and the token expiration time.
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(_config.Duration);

            // Define JWT header as a dictionary of key-value pairs.
            Dictionary<string, string> headers = new()
            {
                //{ JwtHeaderParameterNames.Kid, config.KeyId },   // Key ID used to identify the signing key
                { JwtHeaderParameterNames.Alg, _config.Algorithm }, // Algorithm used for signing (e.g., RS256)
                //{ JwtHeaderParameterNames.Typ, config.Type }     // Type of the token, usually "JWT"
            };

            // Define the payload as a list of claims, which represent the content of the JWT.
            var payloadClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iss, _config.Issuer),   // Issuer of the token
                new Claim(JwtRegisteredClaimNames.Sub, _config.Subject),  // Subject of the token
                new Claim(JwtRegisteredClaimNames.Aud, _config.Audience), // Audience for which the token is intended
                new Claim(OAuth2Consts.PDNDPurposeIdClaimName, _config.PurposeId), // Custom claim for the purpose of the token
                new Claim(JwtRegisteredClaimNames.Jti, tokenId), // JWT ID
                new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimestamp().ToString(), ClaimValueTypes.Integer64), // Issued At time (as Unix timestamp)
                new Claim(JwtRegisteredClaimNames.Exp, expiresAt.ToUnixTimestamp().ToString(), ClaimValueTypes.Integer64)  // Expiration time (as Unix timestamp)
            };

            // Create signing credentials using RSA for signing the token.
            using var rsa = SecurityUtils.GetRsaFromKeyPath(_config.KeyPath);
            var rsaSecurityKey = new RsaSecurityKey(rsa)
            {
              KeyId = _config.KeyId
            };
            var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            // Create the JWT token with the specified header and payload claims.
            var token = new JwtSecurityToken(
                new JwtHeader(signingCredentials, headers, _config.Type),
                new JwtPayload(payloadClaims)
            );

            // Use JwtSecurityTokenHandler to convert the token into a string.
            var tokenHandler = new JwtSecurityTokenHandler();
            string clientAssertion;

            try
            {
                clientAssertion = tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to generate JWT token.", ex);
            }

            return await Task.FromResult(clientAssertion); // Return the generated token as a string.
        }

        /// <summary>
        /// Asynchronously requests an access token by sending the client assertion to the OAuth2 server.
        /// </summary>
        /// <param name="clientAssertion">The client assertion (JWT) used for the token request.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response with the access token as a <see cref="PDNDTokenResponse"/>.</returns>
        public async Task<PDNDTokenResponse> RequestAccessTokenAsync(string clientAssertion)
        {
            using var httpClient = new HttpClient();

            // Create the payload for the POST request in URL-encoded format.
            var payload = new Dictionary<string, string>
            {
                { OpenIdConnectParameterNames.ClientId, _config.ClientId }, // Client ID as per OAuth2 spec
                { OpenIdConnectParameterNames.ClientAssertion, clientAssertion }, // Client assertion (JWT) generated in the previous step
                { OpenIdConnectParameterNames.ClientAssertionType, OAuth2Consts.ClientAssertionTypeJwtBearer }, // Assertion type
                { OpenIdConnectParameterNames.GrantType, OpenIdConnectGrantTypes.ClientCredentials } // Grant type for client credentials
            };

            // Set the Accept header to request JSON responses from the server.
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            // Create the content for the POST request (FormUrlEncodedContent).
            var content = new FormUrlEncodedContent(payload);

            // Send the POST request to the OAuth2 server and await the response.
            var response = await httpClient.PostAsync(_config.ServerUrl, content);

            // Ensure the response indicates success (throws an exception if not).
            response.EnsureSuccessStatusCode();

            // Read and parse the response body as a JSON string.
            var jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                // Deserialize the JSON response into the PDNDTokenResponse object.
                return JsonSerializer.Deserialize<PDNDTokenResponse>(jsonResponse) ?? new PDNDTokenResponse
                {
                  TokenType = string.Empty
                  , AccessToken = string.Empty
                };
            }
            catch (JsonException ex)
            {
                // Handle JSON deserialization errors.
                throw new InvalidOperationException("Failed to deserialize the token response.", ex);
            }
        }
    }
}
