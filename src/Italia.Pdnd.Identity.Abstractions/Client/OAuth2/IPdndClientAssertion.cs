namespace Italia.Pdnd.Identity.Client.OAuth2
{
  public interface IPdndClientAssertion
  {
    /// <summary>
    /// Gets or sets the public key ID (kid)
    /// </summary>
    string KeyId { get; init; }

    /// <summary>
    /// Gets or sets the signing algorithm (alg)
    /// </summary>
    /// <example>"RS256"</example>
    string Algorithm { get; init; }

    /// <summary>
    /// Gets or sets the type of object
    /// </summary>
    /// <example>"JWT"</example>
    string Type { get; init; }

    /// <summary>
    /// Gets or sets the issuer (iss)
    /// </summary>
    string Issuer { get; init; }

    /// <summary>
    /// Gets or sets the subject (sub)
    /// </summary>
    string Subject { get; init; }

    /// <summary>
    /// Gets or sets the audience (aud)
    /// </summary>
    string Audience { get; init; }

    /// <summary>
    /// Gets or sets the purpose (purposeId)
    /// </summary>
    string PurposeId { get; init; }

    /// <summary>
    /// Gets or sets the JWT identifier
    /// </summary>
    Guid TokenId { get; init; }

    /// <summary>
    /// Gets or sets the token creation date
    /// </summary>
    DateTime IssuedAt { get; init; }

    /// <summary>
    /// Gets or sets the token expiration date
    /// </summary>
    DateTime Expiration { get; init; }

    /// <summary>
    /// Gets or sets the client assertion
    /// </summary>
    string ClientAssertion { get; init; }
  }
}