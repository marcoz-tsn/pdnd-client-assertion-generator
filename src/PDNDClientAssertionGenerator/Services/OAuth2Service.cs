// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Italia.Pdnd.IdentityModel.Client.AppConfig;
using Italia.Pdnd.IdentityModel.Client.OAuth2;
using Italia.Pdnd.IdentityModel.ClientAssertionGenerator.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using PdndClaimNames = Italia.Pdnd.IdentityModel.Client.OAuth2.Pdnd.JwtClaimNames;
using PdndHeaderParameterNames = Italia.Pdnd.IdentityModel.Client.OAuth2.Pdnd.JwtHeaderParameterNames;

namespace Italia.Pdnd.IdentityModel.ClientAssertionGenerator.Services
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
        public async Task<string> GenerateClientAssertionAsync(Dictionary<string, string> complementaryInfo)
        {
            // Create signing credentials using RSA for signing the token.
            SigningCredentials signingCredentials;
            using (var rsa = _config.KeyPath != null ?
                     SecurityUtils.GetRsaFromKeyPath(_config.KeyPath) : _config.KeyPem != null ?
                     SecurityUtils.GetRsaFromKeyPem(_config.KeyPem)
                      : throw new InvalidOperationException("No signature key configuration was provided.")
                  )
            {
              var rsaSecurityKey = new RsaSecurityKey(rsa)
              {
                KeyId = _config.KeyId
              };
              signingCredentials = GetSigningCredentials(rsaSecurityKey, _config.Algorithm); //_config.Algorithm -> SecurityAlgorithms.RsaSha256
            }

            // Define the current UTC time and the token expiration time.
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(_config.Duration);

            // Generate a unique token ID (JWS ID)
            var trackingTokenId = Guid.NewGuid().ToString("D").ToLower();
            // Generate a random nonce for augmented entropy
            var dNonce = new Random().NextInt64(1000000000000, 9999999999999).ToString();
            
            var claims = new List<Claim>([
              new Claim(JwtRegisteredClaimNames.Aud, _config.EService), // Audience
              new Claim(JwtRegisteredClaimNames.Iss, _config.Issuer), // Issuer
              new Claim(PdndClaimNames.PurposeId, _config.PurposeId), // Custom claim for the purpose of the token
              new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimestamp().ToString(), ClaimValueTypes.Integer64), // Issued at (numeric)
              new Claim(JwtRegisteredClaimNames.Exp, expiresAt.ToUnixTimestamp().ToString(), ClaimValueTypes.Integer64), // Expiration (numeric)
              new Claim(JwtRegisteredClaimNames.Jti, trackingTokenId), // JWS ID
              new Claim(PdndClaimNames.Dnonce, dNonce, ClaimValueTypes.Integer64),
            ]);
            var trackingEvidence = await GenerateTrackingEvidenceAsync(claims, complementaryInfo, signingCredentials);

            // Compose the tracking digest to add to the payload
            var trackingDigest = ComposeTrackingDigest(trackingEvidence);

            // Generate a unique token ID (JWT ID)
            var tokenId = Guid.NewGuid().ToString("D").ToLower();

            // Define the payload as a list of claims, which represent the content of the JWT.
            var payloadClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iss, _config.Issuer),   // Issuer of the token
                new Claim(JwtRegisteredClaimNames.Sub, _config.Subject),  // Subject of the token
                new Claim(JwtRegisteredClaimNames.Aud, _config.Audience), // Audience for which the token is intended
                new Claim(PdndClaimNames.PurposeId, _config.PurposeId), // Custom claim for the purpose of the token
                new Claim(JwtRegisteredClaimNames.Jti, tokenId), // JWT ID
                new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimestamp().ToString(), ClaimValueTypes.Integer64), // Issued At time (as Unix timestamp)
                new Claim(JwtRegisteredClaimNames.Exp, expiresAt.ToUnixTimestamp().ToString(), ClaimValueTypes.Integer64)  // Expiration time (as Unix timestamp)
            };

            // Define JWT header
            var header = CreateJwtHeader(signingCredentials, _config.Algorithm, _config.Type); //_config.Type -> JwtConstants.HeaderType
            
            // Create the JWT token with the specified header and payload claims.
            var token = new JwtSecurityToken(
                header,
                new JwtPayload(payloadClaims)
            {
              { PdndClaimNames.Digest, trackingDigest } // Add the tracking digest claim to payload
            });

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

            return clientAssertion; // Return the generated token as a string.
        }

        /// <summary>
        /// Asynchronously generates a client assertion (JWT) token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the generated client assertion as a string.</returns>
        public async Task<string> GenerateClientAssertionAsync()
        {
          return await GenerateClientAssertionAsync([]);
        }

        public PDNDTokenRequest GetAccessTokenRequestContent(string clientAssertion)
        {
            var tokenRequest = new PDNDTokenRequest
            {
              // Create the payload for the POST request in URL-encoded format.
              NameValueCollection = new Dictionary<string, string>
              {
                  { OpenIdConnectParameterNames.ClientId, _config.ClientId }, // Client ID as per OAuth2 spec
                  { OpenIdConnectParameterNames.ClientAssertion, clientAssertion }, // Client assertion (JWT) generated in the previous step
                  { OpenIdConnectParameterNames.ClientAssertionType, OAuth2Consts.ClientAssertionTypeJwtBearer }, // Assertion type
                  { OpenIdConnectParameterNames.GrantType, OpenIdConnectGrantTypes.ClientCredentials } // Grant type for client credentials
              }
            };

            // Create the content for the POST request (FormUrlEncodedContent).
            tokenRequest.Content = new FormUrlEncodedContent(tokenRequest.NameValueCollection);

            return tokenRequest;
        }

        /// <summary>
        /// Asynchronously requests an access token by sending the client assertion to the OAuth2 server.
        /// </summary>
        /// <param name="clientAssertion">The client assertion (JWT) used for the token request.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response with the access token as a <see cref="PDNDTokenResponse"/>.</returns>
        public async Task<PDNDTokenResponse> RequestAccessTokenAsync(string clientAssertion)
        {
          // Get the content for the POST request
          var tokenRequest = GetAccessTokenRequestContent(clientAssertion);
          
          // Call overload method with the content
          return await RequestAccessTokenAsync(tokenRequest!);
        }

        /// <summary>
        /// Asynchronously requests an access token by sending the client assertion to the OAuth2 server.
        /// </summary>
        /// <param name="tokenRequest">The HTTP content (FormUrlEncodedContent) used for the token request.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response with the access token as a <see cref="PDNDTokenResponse"/>.</returns>
        public async Task<PDNDTokenResponse> RequestAccessTokenAsync(PDNDTokenRequest tokenRequest)
        {
            using var httpClient = new HttpClient();

            // Set the Accept header to request JSON responses from the server.
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            // Send the POST request to the OAuth2 server and await the response.
            var response = await httpClient.PostAsync(_config.ServerUrl, tokenRequest.Content);

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

        private JwtHeader CreateJwtHeader(SigningCredentials signingCredentials, string securityAlgorithm, string tokenType = JwtConstants.HeaderType)
        {
          Dictionary<string, string> headerParameters = new()
          {
            [JwtHeaderParameterNames.Alg] = securityAlgorithm,
          };
          var header = new JwtHeader(signingCredentials, headerParameters, tokenType);
          return header;
        }

        private SigningCredentials GetSigningCredentials(RsaSecurityKey rsaSecurityKey, string securityAlgorithm)
        {
          var signingCredentials = new SigningCredentials(rsaSecurityKey, securityAlgorithm)
          {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
          };
          return signingCredentials;
        }

        /// <summary>
        /// Asynchronously generates a tacking evidence (JWS) token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the generated tacking evidence as a string.</returns>
        private Task<string> GenerateTrackingEvidenceAsync(List<Claim> claims, Dictionary<string, string> complementaryInfo, SigningCredentials signingCredentials)
        {
          var trackingPayload = new JwtPayload(claims);
          trackingPayload.AddClaims(complementaryInfo.Select(x => new Claim(x.Key, x.Value)));

          // Build and serialize the Tracking JWS.
          var header = CreateJwtHeader(signingCredentials, _config.Algorithm, _config.Type);
          var trackingToken = new JwtSecurityToken(header, trackingPayload);

          // Use JwtSecurityTokenHandler to convert the token into a string.
          var tokenHandler = new JwtSecurityTokenHandler();
          string trackingTokenString;

          try
          {
            trackingTokenString = tokenHandler.WriteToken(trackingToken);
          }
          catch (Exception ex)
          {
            throw new InvalidOperationException("Failed to generate JWS token.", ex);
          }

          return Task.FromResult(trackingTokenString); // Return the generated token as a string.
        }

        private Dictionary<string, string> ComposeTrackingDigest(string trackingTokenString)
        {
          // Calculate hash for digest claim
          string trackingHash;
          using (var sha256 = SHA256.Create())
          {
            trackingHash = GetHash(sha256, trackingTokenString);
          }
          // Create the digest structure
          var digest = new Dictionary<string, string> {
            { JwtHeaderParameterNames.Alg, SecurityAlgorithms.Sha256 },
            { PdndHeaderParameterNames.Value, trackingHash }
          };

          return digest;
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
          // Convert the input string to a byte array and compute the hash.
          var data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

          // Create a new StringBuilder to collect the bytes and create a string.
          var sBuilder = new StringBuilder();

          // Loop through each byte of the hashed data and format each one as a hexadecimal string.
          for (var i = 0; i < data.Length; i++)
          {
            sBuilder.Append(data[i].ToString("x2"));
          }

          // Return the hexadecimal string.
          return sBuilder.ToString();
        }
    }
}
