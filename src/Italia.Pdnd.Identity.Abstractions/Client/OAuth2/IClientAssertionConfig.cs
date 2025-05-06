namespace Italia.Pdnd.Identity.Client.OAuth2
{
  public interface IClientAssertionConfig
  {
    /// <summary>
    /// Gets or sets the authentication server URL
    /// </summary>
    string ServerUrl { get; set; }

    /// <summary>
    /// Gets or sets the public key ID (kid)
    /// </summary>
    string KeyId { get; set; }

    /// <summary>
    /// Gets or sets the signing algorithm (alg)
    /// </summary>
    /// <remarks>Actually only RS256 is available</remarks>
    /// <example>"RS256"</example>
    string Algorithm { get; set; }

    /// <summary>
    /// Gets or sets the type of object
    /// </summary>
    /// <example>"JWT"</example>
    string Type { get; set; }

    /// <summary>
    /// Gets or sets the Client identifier
    /// </summary>
    string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the issuer (iss)
    /// </summary>
    /// <remarks>Should be set as the client ID</remarks>
    string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the subject (sub)
    /// </summary>
    /// <remarks>Should be set as the client ID</remarks>
    string Subject { get; set; }

    /// <summary>
    /// Gets or sets the audience (aud)
    /// </summary>
    string Audience { get; set; }

    /// <summary>
    /// Gets or sets the purpose for which access to resources will be requested (purposeId)
    /// </summary>
    string PurposeId { get; set; }

    /// <summary>
    /// Gets or sets the path to the private key to sign the client assertion
    /// </summary>
    string KeyPath { get; set; }

    /// <summary>
    /// Gets or sets the duration in minutes of the token (this will be used to calculate the token expiration)
    /// </summary>
    int Duration { get; set; }
  }
}