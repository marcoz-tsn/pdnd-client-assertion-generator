// (c) 2024 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)

using Italia.Pdnd.IdentityModel.Client.OAuth2;
using Italia.Pdnd.IdentityModel.ClientAssertionGenerator.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Italia.Pdnd.IdentityModel.ClientAssertionGenerator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientAssertionController : ControllerBase
    {
        private readonly IClientAssertionGenerator _clientAssertionGenerator;
        private readonly ILogger<ClientAssertionController> _logger;

        public ClientAssertionController(ILogger<ClientAssertionController> logger, IClientAssertionGenerator clientAssertionGenerator)
        {
            _logger = logger;
            _clientAssertionGenerator = clientAssertionGenerator;
        }

        [HttpGet("GetClientAssertion", Name = "GetClientAssertion")]
        public async Task<string> GetClientAssertionAsync()
        {
            return await _clientAssertionGenerator.GetClientAssertionAsync();
        }

        [HttpGet("GetToken", Name = "GetToken")]
        public async Task<PDNDTokenResponse> GetToken([FromQuery] string clientAssertion)
        {
            return await _clientAssertionGenerator.GetTokenAsync(clientAssertion);
        }
    }
}
