using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.AspNetCore.Mvc;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using GoogleIncrementalSample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GoogleIncrementalSample.Controllers
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
        /// If the user is signed in, load ViewData with some information to display.
        /// </summary>
        private async Task LoadUserInfo(CancellationToken cancellationToken)
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
            }
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
            // Ask the user to sign in. Scopes requested: email and profile.
            var result =
                await new AuthorizationCodeMvcApp(this, new SignInAppFlowMetadata(ClientId, ClientSecret))
                    .AuthorizeAsync(cancellationToken)
                    .ConfigureAwait(false);

            if (result.Credential == null)
            {
                return Redirect(result.RedirectUri);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// User Sign Out action.
        /// </summary>
        public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
        {
            var appFlow = new ClassListAppFlowMetadata(ClientId, ClientSecret);
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
            // Simplify the incremental auth experience by providing a login_hint. The user will
            // not be asked to select their account if they have already signed in.
            await LoadUserInfo(cancellationToken);
            var flowData = new ClassListAppFlowMetadata(ClientId, ClientSecret);
            if (!string.IsNullOrEmpty(ViewData["PersonEmail"]?.ToString()))
            {
                flowData.LoginHint = ViewData["PersonEmail"].ToString();
            }

            // Incremental authorization
            var result = await new AuthorizationCodeMvcApp(this, flowData)
                .AuthorizeAsync(cancellationToken)
                .ConfigureAwait(false);

            if (result.Credential == null)
            {
                return Redirect(result.RedirectUri);
            }

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
    }
}
