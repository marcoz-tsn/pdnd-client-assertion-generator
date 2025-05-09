namespace Italia.Pdnd.Identity.Client.OAuth2;

public class PDNDVoucher : PDNDTokenResponse
{
  public string TrackingEvidence { get; set; }
  public string Digest { get; set; }
}