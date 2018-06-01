using System;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Classroom.v1;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;

namespace GoogleIncrementalMvcSample.Controllers
{
    /// <summary>
    /// This flow is used to retrieve the user's Google Classroom class list. It needs an additional scope
    /// compared to the SignIn flow, which is requested as an incremental authorization.
    /// </summary>
    public class ClassListAppFlowMetadata : FlowMetadata, IAppFlowScopes
    {
        /// <summary>
        /// Create a new FlowMetadata with additional scopes to access Classroom data without a login_hint.
        /// </summary>
        /// <param name="clientId">The ClientId for this app.</param>
        /// <param name="clientSecret">The ClientSecret for this app.</param>
        public ClassListAppFlowMetadata(string clientId, string clientSecret) : this(clientId, clientSecret, null) {}

        /// <summary>
        /// Create a new FlowMetadata with additional scopes to access Classroom data.
        /// </summary>
        /// <param name="clientId">The ClientId for this app.</param>
        /// <param name="clientSecret">The ClientSecret for this app.</param>
        /// <param name="loginHint">The login_hint when requesting an authorization code</param>
        public ClassListAppFlowMetadata(string clientId, string clientSecret, string loginHint)
        {
            Flow =
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    Scopes = new[]
                    {
                        "email",
                        "profile",
                        ClassroomService.Scope.ClassroomCoursesReadonly
                    },

                    // Using a FileDataStore for Development
                    DataStore = new FileDataStore("Classroom.Api.Auth.Store"),

                    // Set the login_hint
                    LoginHint = string.IsNullOrEmpty(loginHint) 
                        ? null 
                        : loginHint
                });
        }

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.

            var user = controller.TempData.Peek("user");
            if (user == null)
            {
                user = Guid.NewGuid();
                controller.TempData["user"] = user;
            }
            return user.ToString();
        }

        public override string AuthCallback
        {
            // This must match a Redirect URI for the Client ID you are using
            get { return @"/ClassListAuthCallback"; }
        }

        public override IAuthorizationCodeFlow Flow { get; }

        public IEnumerable<string> Scopes
        {
            get { return ((GoogleAuthorizationCodeFlow)Flow).Scopes; }
        }
    }
}
