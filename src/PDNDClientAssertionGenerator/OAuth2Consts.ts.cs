public static class OAuth2Consts
{
  // Claim names
  public const string PDNDPurposeIdClaimName = "purposeId";

  // Common values
  public const string TokenEndpointUat = "https://auth.uat.interop.pagopa.it/token.oauth2";
  public const string TokenEndpointProduction = "https://auth.interop.pagopa.it/token.oauth2";

  public const string ClientAssertionTypeJwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
}
