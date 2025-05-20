// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Italia.Pdnd.IdentityModel.Client.OAuth2
{
    public interface IOAuth2Service
    {
        Task<string> GenerateClientAssertionAsync();
        Task<PDNDTokenResponse> RequestAccessTokenAsync(string clientAssertion);
        Task<PDNDVoucherMetadata> GenerateClientAssertionAsync(Dictionary<string, string> complementaryInfo);
        PDNDAccessTokenRequest GetAccessTokenRequest(PDNDVoucherMetadata voucherMetadata);
        Task<PDNDVoucher> RequestAccessTokenAsync(PDNDAccessTokenRequest accessTokenRequest);
    }
}