namespace Italia.Pdnd.IdentityModel.Client.OAuth2
{
  //public class PDNDVoucher : PDNDTokenResponse
  public class PDNDVoucher
  {
    //public string TrackingEvidence { get; set; }
    //public string Digest { get; set; }

    public PDNDVoucherMetadata? VoucherMetadata { get; set; }
    public PDNDTokenResponse? TokenResponse { get; set; }
  }
}