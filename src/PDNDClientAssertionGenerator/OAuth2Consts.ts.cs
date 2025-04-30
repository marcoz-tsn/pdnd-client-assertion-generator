public static class OAuth2Consts
{
  // Parameter names
  public const string PDNDValueParameterName = "value";

  // Claim names
  public const string PDNDPurposeIdClaimName = "purposeId";
  public const string PDNDDigestClaimName = "digest";

  // Common values
  public const string TokenEndpointUat = "https://auth.uat.interop.pagopa.it/token.oauth2";
  public const string TokenEndpointProduction = "https://auth.interop.pagopa.it/token.oauth2";

  public const string ClientAssertionTypeJwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
}
