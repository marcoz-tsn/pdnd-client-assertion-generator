namespace Italia.Pdnd.IdentityModel.Client.OAuth2;

public class PDNDVoucher : PDNDTokenResponse
{
  public string TrackingEvidence { get; set; }
  public string Digest { get; set; }
}