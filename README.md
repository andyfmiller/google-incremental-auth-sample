# Google Incremental Authorization Sample
A web app to demonstrate incremental authorization. 
This project requires a modified version of Google's API Client Library for .NET.

## Getting Started
1. Clone this repository.
2. Clone https://github.com/andyfmiller/google-api-dotnet-client.
3. Open GoogleIncrementalSample.sln in VS 2017 and fix the project references to Google.Apis.Auth and Google.Apis.Auth.AspNetCore.
4. Right click on the project and select Manage User Secrets.
5. Fill in your ClientId and ClientSecret:
```
{
  "Authentication:Google:ClientId": "",
  "Authentication:Google:ClientSecret": ""
}
```
6. Run the solution!
a. First sign in. This will only request the email and profile scopes.
b. Second list your Google Classroom classes. This will incrementally request the ClassroomService.Scope.ClassroomCoursesReadonly scope.
