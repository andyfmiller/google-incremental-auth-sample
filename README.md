# Google Incremental Authorization Samples
Two samples ASP.NET Core 2.0 web apps to demonstrate incremental authorization. Google
[recommends](https://developers.google.com/identity/sign-in/web/incremental-auth) requesting scopes
only as needed, especially if consent screen with all scopes is overwhelming. This is often the
case with educational apps which require consent from students.

1. [GoogleIncrementalMvcSample](https://github.com/andyfmiller/google-incremental-auth-sample/tree/master/src/GoogleIncrementalMvcSample) uses the [Google APIs Client Library for .NET](https://developers.google.com/api-client-library/dotnet) with a small wrapper library to support AspNetCore 2.0. This sample follows the example shown for Web Apps on the [OAuth 2.0 Authentication and Authorization](https://developers.google.com/api-client-library/dotnet/guide/aaa_oauth) page.
2. [MicrosoftIncrementalMvcSample](https://github.com/andyfmiller/google-incremental-auth-sample/tree/master/src/MicrosoftIncrementalMvcSample) uses Microsoft's ASP.NET Core Authentication middleware to acquire the tokens, and [Google APIs Client Library for .NET](https://developers.google.com/api-client-library/dotnet) to use Google Services.

## Getting Started with the GoogleIncrementalMvcSamples
1. Clone this repository.
3. Open GoogleIncrementalSamples.sln in VS 2017 and make sure you can build the solution.
4. Right click on the GoogleIncrementalMvcSample project and select Manage User Secrets.
5. Fill in your ClientId and ClientSecret:
```
{
  "Authentication:Google:ClientId": "",
  "Authentication:Google:ClientSecret": ""
}
```
5. Mark GoogleIncrementalMvcSample as the Startup Project.
6. Run the solution!
    1. First sign in. This will only request the email and profile scopes.
    2. Second list your Google Classroom classes. This will incrementally request the ClassroomService.Scope.ClassroomCoursesReadonly scope.
