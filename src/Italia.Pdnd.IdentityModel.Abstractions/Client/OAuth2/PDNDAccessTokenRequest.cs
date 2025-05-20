namespace Italia.Pdnd.IdentityModel.Client.OAuth2;

public class PDNDAccessTokenRequest
{
  public required IEnumerable<KeyValuePair<string, string>> NameValueCollection { get; set; }

  public HttpContent? Content { get; set; }

  public PDNDVoucherMetadata VoucherMetadata { get; set; }
}