using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using GoogleIncrementalMvcSample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GoogleIncrementalMvcSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ClientId
        {
            get { return _configuration["Authentication:Google:ClientId"]; }
        }

        private string ClientSecret
        {
            get { return _configuration["Authentication:Google:ClientSecret"]; }
        }

        /// <summary>
        /// Display the home page.
        /// </summary>
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            await LoadUserInfo(cancellationToken);

            return View();
        }

        /// <summary>
        /// Display an error page.
        /// </summary>
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        /// <summary>
        /// User Sign In action.
        /// </summary>
        public async Task<IActionResult> SignIn(CancellationToken cancellationToken)
        {
            var appFlow = new SignInAppFlowMetadata(ClientId, ClientSecret);

            // Ask the user to sign in. Scopes requested: email and profile.
            var result =
                await new AuthorizationCodeMvcApp(this, appFlow)
                    .AuthorizeAsync(cancellationToken)
                    .ConfigureAwait(false);

            if (result.Credential == null)
            {
                return Redirect(result.RedirectUri);
            }


            // Store the scopes in the token
            await AddScopesToTokenAsync(appFlow, cancellationToken).ConfigureAwait(false);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// User Sign Out action.
        /// </summary>
        public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
        {
            var appFlow = new SignInAppFlowMetadata(ClientId, ClientSecret);
            var userId = appFlow.GetUserId(this);
            var token = await appFlow.Flow.LoadTokenAsync(userId, cancellationToken);
            if (token != null && !string.IsNullOrEmpty(token.AccessToken))
            {
                await appFlow.Flow.RevokeTokenAsync(userId, token.AccessToken, cancellationToken).ConfigureAwait(false);
                TempData.Remove("user");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Display the list of the the user's Google Classroom classes. Scopes
        /// requested: email, profile, and ClassroomService.Scope.ClassroomCoursesReadonly.
        /// </summary>
        public async Task<IActionResult> ListCourses(CancellationToken cancellationToken)
        {
            var loginHint = await GetUserEmail(cancellationToken).ConfigureAwait(false);
            var appFlow = new ClassListAppFlowMetadata(ClientId, ClientSecret, loginHint);
            var userId = appFlow.GetUserId(this);
            var token = await appFlow.Flow.LoadTokenAsync(userId, cancellationToken).ConfigureAwait(false);

            // If the current token does not include all the required scopes,
            // expire the token to force a new authorization code. If the scopes
            // are null, then this is a fresh token, we don't need a new one.
            if (token != null && !string.IsNullOrWhiteSpace(token.Scope))
            {
                var tokenScopes = token.Scope.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (!appFlow.Scopes.All(s => tokenScopes.Contains(s)))
                {
                    token.RefreshToken = null;
                    token.ExpiresInSeconds = 0;
                    await appFlow.Flow.DataStore.StoreAsync(userId, token);
                }
            }

            var result = await new AuthorizationCodeMvcApp(this, appFlow)
                .AuthorizeAsync(cancellationToken)
                .ConfigureAwait(false);

            if (result.Credential == null)
            {
                return Redirect(result.RedirectUri);
            }

            // Store the scopes in the token
            await AddScopesToTokenAsync(appFlow, cancellationToken).ConfigureAwait(false);

            // Load the new user information
            await LoadUserInfo(cancellationToken).ConfigureAwait(false);

            var model = new CoursesModel();

            try
            {
                using (var classroomService = new ClassroomService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "gc2lti"
                }))
                {
                    // Get the list of the user's courses
                    model.Courses = new List<CourseModel>();

                    var coursesRequest = classroomService.Courses.List();
                    coursesRequest.CourseStates = CoursesResource.ListRequest.CourseStatesEnum.ACTIVE;
                    coursesRequest.TeacherId = "me";

                    ListCoursesResponse coursesResponse = null;
                    do
                    {
                        if (coursesResponse != null)
                        {
                            coursesRequest.PageToken = coursesResponse.NextPageToken;
                        }

                        coursesResponse =
                            await coursesRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                        if (coursesResponse.Courses != null)
                        {
                            foreach (var course in coursesResponse.Courses)
                            {
                                model.Courses.Add(new CourseModel
                                {
                                    CourseId = course.Id,
                                    CourseName = course.Name
                                });
                            }
                        }
                    } while (!string.IsNullOrEmpty(coursesResponse.NextPageToken));

                    return View("Index", model);
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        private async Task<string> GetUserEmail(CancellationToken cancellationToken)
        {
            try
            {
                var appFlow = new SignInAppFlowMetadata(ClientId, ClientSecret);
                var userId = appFlow.GetUserId(this);
                var token = await appFlow.Flow.LoadTokenAsync(userId, cancellationToken).ConfigureAwait(false);
                if (token != null && !string.IsNullOrEmpty(token.IdToken))
                {
                    var payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken).ConfigureAwait(false);
                    return payload.Email;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        /// <summary>
        /// If the user is signed in, load ViewData with some information to display.
        /// </summary>
        private async Task LoadUserInfo(CancellationToken cancellationToken)
        {
            try
            {
                var appFlow = new SignInAppFlowMetadata(ClientId, ClientSecret);
                var userId = appFlow.GetUserId(this);
                var token = await appFlow.Flow.LoadTokenAsync(userId, cancellationToken).ConfigureAwait(false);
                if (token != null && !string.IsNullOrEmpty(token.IdToken))
                {
                    var payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken).ConfigureAwait(false);
                    ViewData["PersonName"] = payload.Name;
                    ViewData["PersonEmail"] = payload.Email;
                    ViewData["PersonPicture"] = payload.Picture;
                    ViewData["GrantedScopes"] = token.Scope;
                }
                ViewData["SignInScopes"] = string.Join(" ", appFlow.Scopes);
                var classListAppFlow = new ClassListAppFlowMetadata(ClientId, ClientSecret);
                ViewData["ClassListScopes"] = string.Join(" ", classListAppFlow.Scopes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task AddScopesToTokenAsync(IAppFlowScopes appFlow, CancellationToken cancellationToken)
        {
            var userId = appFlow.GetUserId(this);
            var token = await appFlow.Flow.LoadTokenAsync(userId, cancellationToken).ConfigureAwait(false);
            token.Scope = string.Join(" ", appFlow.Scopes);
            await appFlow.Flow.DataStore.StoreAsync(userId, token).ConfigureAwait(false);
        }
    }
}
