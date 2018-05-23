using System.Collections.Generic;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Mvc;

namespace GoogleIncrementalMvcSample.Controllers
{
    interface IAppFlowScopes
    {
        /// <summary>Gets the authorization code flow.</summary>
        IAuthorizationCodeFlow Flow { get; }

        string GetUserId(Controller controller);

        /// <summary>Get the authorization code flow scopes.</summary>
        IEnumerable<string> Scopes { get; }
    }
}
