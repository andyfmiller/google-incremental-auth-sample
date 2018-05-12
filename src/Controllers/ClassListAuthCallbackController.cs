using Google.Apis.Auth.OAuth2.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GoogleIncrementalSample.Controllers
{
    /// <summary>
    /// Handle the Class List AuthCallback request.
    /// </summary>
    [Route("[controller]")]
    public class ClassListAuthCallbackController : AuthCallbackController
    {
        private readonly IConfiguration _configuration;

        private string ClientId
        {
            get { return _configuration["Authentication:Google:ClientId"]; }
        }

        private string ClientSecret
        {
            get { return _configuration["Authentication:Google:ClientSecret"]; }
        }

        public ClassListAuthCallbackController(IConfiguration config)
        {
            _configuration = config;
        }

        protected override FlowMetadata FlowData
        {
            get
            {
                return new ClassListAppFlowMetadata(ClientId, ClientSecret);
            }
        }
    }
}