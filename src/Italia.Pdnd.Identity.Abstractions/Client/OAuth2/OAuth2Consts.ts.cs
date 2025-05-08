namespace Italia.Pdnd.Identity.Client.OAuth2
{
  public static class OAuth2Consts
{
  // Header names
  public const string PDNDTrackingEvidenceHeaderName = "Agid-JWT-TrackingEvidence";
  public const string PDNDSignatureHeaderName = "Agid-JWT-Signature";

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
}
