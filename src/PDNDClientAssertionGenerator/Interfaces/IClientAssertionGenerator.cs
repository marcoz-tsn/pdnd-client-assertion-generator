// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)

using Italia.Pdnd.IdentityModel.Client.OAuth2;

namespace Italia.Pdnd.IdentityModel.ClientAssertionGenerator.Interfaces
{
    public interface IClientAssertionGenerator
    {
        Task<string> GetClientAssertionAsync();
        Task<PDNDTokenResponse> GetTokenAsync(string clientAssertion);
    }
}
