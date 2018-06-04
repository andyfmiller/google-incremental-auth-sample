using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MicrosoftIncrementalMvcSample.Models;

namespace MicrosoftIncrementalMvcSample.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Display the home page.
        /// </summary>
        public IActionResult Index()
        {
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
        /// Force simple sign in.
        /// </summary>
        public IActionResult SignIn()
        {
            return new ChallengeResult(new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index")
            });
        }

        /// <summary>
        /// Sign out of this site. You are still signed in to Google.
        /// </summary>
        public async Task<IActionResult> SignOutOfThisApp()
        {
            await HttpContext.SignOutAsync().ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Sign out of this site and Google. If you open another tab, you will be signed out of Google.
        /// </summary>
        public async Task<IActionResult> SignOutOfGoogle()
        {
            // Sign out of this site.
            await HttpContext.SignOutAsync().ConfigureAwait(false);

            //// Sign out of Google (after signout, redirects to Google Sign In)
            //var redirectUrl = "https://www.google.com/accounts/Logout";

            // Sign out of Google (after signout, redirects to Google home page)
            //var redirectUrl = "https://www.google.com/accounts/Logout?continue=https://www.google.com";

            // Sign out of Google (after signout, redirects to this app's home page)
            var redirectUrl = "https://www.google.com/accounts/Logout?continue=https://appengine.google.com/_ah/logout?continue="
                              + Request.Scheme + "://" + Request.Host + Url.Action("Index");

            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Display the list of the the user's Google Classroom classes. Scopes
        /// requested: email, profile, and ClassroomService.Scope.ClassroomCoursesReadonly.
        /// </summary>
        public async Task<IActionResult> ListCourses(CancellationToken cancellationToken)
        {
            // If they haven't signed in yet, do that first
            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(new AuthenticationProperties
                {
                    RedirectUri = Url.Action("ListCourses")
                });
            }

            // Simplify the incremental auth experience by providing a login_hint. The user will
            // not be asked to select their account if they have already signed in.
            var loginHint = GetUserEmail();
            var accessToken = await HttpContext.GetTokenAsync("ClassList", "access_token");
            var expires = await HttpContext.GetTokenAsync("ClassList", "expires_at");
            DateTime.TryParse(expires, out var expiresAt);

            if (accessToken == null || DateTime.Now > expiresAt)
            {
                return new ChallengeResult("ClassList", new AuthenticationProperties()
                {
                    Parameters =
                    {
                        new KeyValuePair<string, object>("login_hint", loginHint ),
                        new KeyValuePair<string, object>("include_granted_scopes", true)
                    },
                    RedirectUri = Url.Action("ListCourses")
                });
            }

            var model = new CoursesModel();

            try
            {
                using (var classroomService = new ClassroomService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken),
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

        /// <summary>
        /// Get the user's email from the current token
        /// </summary>
        private string GetUserEmail()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            }

            return null;
        }
    }
}
