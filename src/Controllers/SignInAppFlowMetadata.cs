using System;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;

namespace GoogleIncrementalSample.Controllers
{
    /// <summary>
    /// This flow is used to sign in. Only email and profile scopes are requested.
    /// </summary>
    public class SignInAppFlowMetadata : FlowMetadata
    {
        public SignInAppFlowMetadata(string clientId, string clientSecret)
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
                        "profile"
                    },

                    // Using a FileDataStore for Development
                    DataStore = new FileDataStore("Classroom.Api.Auth.Store"),

                    // Force the user to select an account when they sign in
                    UserDefinedQueryParams = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("prompt", "select_account")
                    }
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
            get { return @"/SignInAuthCallback"; }
        }

        public override IAuthorizationCodeFlow Flow { get; }
    }
}
