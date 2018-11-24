using System.Collections.Generic;

namespace GoogleIncrementalMvcSample.Models
{
    public class IndexModel
    {
        public IList<CourseModel> Courses { get; set; }
        public IReadOnlyList<string> Scopes { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
    }
}
