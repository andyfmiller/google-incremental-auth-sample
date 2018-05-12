# Google Classroom Bulk Grader
A web app to update a bunch of grades in a Google Classroom course.
You can only update the grades for assignments made by the same Google project.

## Getting Started
1. Close this project.
2. Clone @buzallens's https://github.com/buzallen/google-api-dotnet-client/tree/master/Src/Support/Google.Apis.Auth.AspMvcCore project.
3. Open gcbulkgrader.sln in VS 2017 and fix the Google.Apis.Auth.AspMvcCore project so it loads from your clone.
4. Right click on the gcbulkgrader project and select Manage User Secrets.
5. Fill in your ClientId and ClientSecret:
```
{
  "Authentication:Google:ClientId": "",
  "Authentication:Google:ClientSecret": ""
}
```
6. Run the solution!
