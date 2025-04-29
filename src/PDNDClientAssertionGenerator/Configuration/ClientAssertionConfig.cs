// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace PDNDClientAssertionGenerator.Configuration
{
    public class ClientAssertionConfig
    {
        /// <summary>
        /// Gets or sets the authentication server URL
        /// <remarks>IW: Credential.Url ?? Connector.Url</remarks>
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the public key ID (kid)
        /// <remarks>PDND: Fruizione / I tuoi client e-service / Gestisci client e-service / Chiavi pubbliche / ID chiave</remarks>
        /// </summary>
        public string KeyId { get; set; }

        /// <summary>
        /// Gets or sets the signing algorithm (alg)
        /// </summary>
        /// <remarks>Actually only RS256 is available</remarks>
        /// <example>"RS256"</example>
        public string Algorithm { get; set; }

        /// <summary>
        /// Gets or sets the type of object
        /// </summary>
        /// <remarks>Always JWT</remarks>
        /// <example>"JWT"</example>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Client identifier
        /// </summary>
        /// <remarks>PDND: Fruizione / I tuoi client e-service / Gestisci client e-service / Client Assertion / ID DEL CLIENT</remarks>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the issuer (iss)
        /// </summary>
        /// <remarks>Should be set as the client ID</remarks>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the subject (sub)
        /// </summary>
        /// <remarks>Should be set as the client ID</remarks>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the audience (aud)
        /// <remarks>PDND: Fruizione / I tuoi client e-service / Gestisci client e-service / Client Assertion / AUDIENCE</remarks>
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the purpose for which access to resources will be requested (purposeId)
        /// <remarks>PDND: Fruizione / I tuoi client e-service / Gestisci client e-service / Client Assertion / ID DELLA FINALITÀ</remarks>
        /// </summary>
        public string PurposeId { get; set; }

        /// <summary>
        /// Gets or sets the path to the private key to sign the client assertion
        /// <remarks>IW: TBD (Credential.CertificateKey ?)</remarks>
        /// </summary>
        public string KeyPath { get; set; }

        /// <summary>
        /// Gets or sets the duration in minutes of the token (this will be used to calculate the token expiration)
        /// <remarks>IW: TBD (Credential.[NEW_PROP]] ?)</remarks>
        /// </summary>
        public int Duration { get; set; }
    }
}
