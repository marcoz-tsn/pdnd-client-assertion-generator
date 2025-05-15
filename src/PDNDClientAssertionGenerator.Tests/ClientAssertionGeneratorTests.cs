// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)

using Italia.Pdnd.IdentityModel.Client.OAuth2;
using Italia.Pdnd.IdentityModel.ClientAssertionGenerator.Services;
using Moq;

namespace Italia.Pdnd.IdentityModel.ClientAssertionGenerator.Tests
{
    public class ClientAssertionGeneratorServiceTests
    {
        [Fact]
        public async Task GetClientAssertionAsync_ShouldCall_GenerateClientAssertionAsync()
        {
            // Arrange
            var oauth2ServiceMock = new Mock<IOAuth2Service>();
            var clientAssertionGeneratorService = new ClientAssertionGeneratorService(oauth2ServiceMock.Object);

            // Act
            await clientAssertionGeneratorService.GetClientAssertionAsync();

            // Assert
            oauth2ServiceMock.Verify(o => o.GenerateClientAssertionAsync(), Times.Once);
        }

        [Fact]
        public async Task GetToken_ShouldCall_RequestAccessTokenAsync_WithClientAssertion()
        {
            // Arrange
            var oauth2ServiceMock = new Mock<IOAuth2Service>();
            var clientAssertionGeneratorService = new ClientAssertionGeneratorService(oauth2ServiceMock.Object);
            var clientAssertion = "testClientAssertion";

            // Act
            await clientAssertionGeneratorService.GetTokenAsync(clientAssertion);

            // Assert
            oauth2ServiceMock.Verify(o => o.RequestAccessTokenAsync(clientAssertion), Times.Once);
        }
    }
}