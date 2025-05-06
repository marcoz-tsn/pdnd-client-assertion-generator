namespace Italia.Pdnd.Identity.Client.OAuth2
{
  public interface IPdndTokenResponse : IOAuth2Response
  {
    int ExpiresIn { get; set; }
    string AccessToken { get; set; }
  }
}