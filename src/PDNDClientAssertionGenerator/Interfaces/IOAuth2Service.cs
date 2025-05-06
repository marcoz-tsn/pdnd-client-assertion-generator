// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using PDNDClientAssertionGenerator.Configuration;
using PDNDClientAssertionGenerator.Models;

namespace PDNDClientAssertionGenerator.Interfaces
{
    public interface IOAuth2Service
    {
        Task<string> GenerateClientAssertionAsync();
        Task<PDNDTokenResponse> RequestAccessTokenAsync(string clientAssertion);

        Task<string> GenerateClientAssertionAsync(string tokenId, ClientAssertionConfig config);
        Task<string> GenerateClientAssertionAsync(string tokenId);
        Task<PDNDTokenResponse> RequestAccessTokenAsync(string clientId, string clientAssertion);
    }
}