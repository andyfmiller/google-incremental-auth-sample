# Google Incremental Authorization Samples
Three samples ASP.NET Core 2.1 web apps to demonstrate incremental authorization. Google
[recommends](https://developers.google.com/identity/sign-in/web/incremental-auth) requesting scopes
only as needed, especially if the consent screen with all scopes is overwhelming. This is often the
case with educational apps which require consent from students.

1. [GoogleIncrementalMvcSample](https://github.com/andyfmiller/google-incremental-auth-sample/tree/master/src/GoogleIncrementalMvcSample) uses the [Google APIs Client Library for .NET](https://developers.google.com/api-client-library/dotnet) with a small custom library to support ASP.NET Core. This sample follows the example shown for Web Apps on the [OAuth 2.0 Authentication and Authorization](https://developers.google.com/api-client-library/dotnet/guide/aaa_oauth) page.
2. [MicrosoftIncrementalMvcSample](https://github.com/andyfmiller/google-incremental-auth-sample/tree/master/src/MicrosoftIncrementalMvcSample) uses Microsoft's ASP.NET Core OAuth2 middleware to acquire the tokens, and [Google APIs Client Library for .NET](https://developers.google.com/api-client-library/dotnet) to use Google Services.
3. [OpenIdConnectIncrementalMvcSample](https://github.com/andyfmiller/google-incremental-auth-sample/tree/master/src/OpenIdConnectIncrementalMvcSample) uses Microsoft's ASP.NET Core OpenID Connect middleware to authenticate the user, OAuth 2 middleware to get authorization from the user to access their classes, and [Google APIs Client Library for .NET](https://developers.google.com/api-client-library/dotnet) to use Google Services.
