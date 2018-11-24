using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth.AspNetCore;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using GoogleIncrementalMvcSample.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoogleIncrementalMvcSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGoogleAuthProvider _auth;

        public HomeController(IGoogleAuthProvider auth)
        {
            _auth = auth;
        }

        /// <summary>
        /// Display the home page.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var model = await LoadUserInfoAsync();

            return View(model);
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
        [Authorize]
        public IActionResult SignIn()
        {
            return RedirectToAction("Index");
        }

        /// <summary>
        /// User Sign Out action.
        /// </summary>
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Display the list of the the user's Google Classroom classes. Scopes
        /// requested: email, profile, and ClassroomService.Scope.ClassroomCoursesReadonly.
        /// </summary>
        [GoogleScopedAuthorize("https://www.googleapis.com/auth/classroom.courses.readonly")]
        public async Task<IActionResult> ListCourses()
        {
            var cred = await _auth.GetCredentialAsync();

            var model = await LoadUserInfoAsync();

            try
            {
                using (var classroomService = new ClassroomService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = cred,
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
                            await coursesRequest.ExecuteAsync();
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

        private async Task<IndexModel> LoadUserInfoAsync()
        {
            var model = new IndexModel();

            if (User.Identity.IsAuthenticated)
            {
                model.UserEmail = User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                model.UserName = User.Claims.SingleOrDefault(c => c.Type == "name")?.Value;

                model.Scopes = await _auth.GetCurrentScopesAsync();
            }

            return model;
        }
    }
}
