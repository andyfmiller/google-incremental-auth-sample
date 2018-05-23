using System.Collections.Generic;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Mvc;

namespace GoogleIncrementalMvcSample.Controllers
{
    internal interface IAppFlowScopes
    {
        /// <summary>Gets the authorization code flow.</summary>
        IAuthorizationCodeFlow Flow { get; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <param name="controller">The controller</param>
        /// <returns>User identifier</returns>
        string GetUserId(Controller controller);

        /// <summary>Get the authorization code flow scopes.</summary>
        IEnumerable<string> Scopes { get; }
    }
}
