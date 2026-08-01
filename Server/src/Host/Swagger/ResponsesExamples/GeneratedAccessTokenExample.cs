using Host.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Host.Swagger.ResponsesExamples
{
    public class GeneratedAccessTokenExample : IExamplesProvider<GeneratedAccessToken>
    {
        public GeneratedAccessToken GetExamples()
        {
            return new("eyJhbGciOiJIUzI1NiIsImtpZCI6IjcxNTFkMWQxLWI5NzEtNDhiNC1hMzQ1LWU4OTFkZjgyYmY3ZiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJjbGllbnQiLCJpc3MiOiJsb2NhbGhvc3QiLCJleHAiOjE3ODQ3MDI3MzEsImlhdCI6MTc4NDcwMDkzMSwibmJmIjoxNzg0NzAwOTMxLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjA2MWQ0ZTVhLTU5MjctNDc0MS05MzA5LWE4Mzc0YTllM2NiMyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIifQ.YlXNz-YAO0zhnFxgH5z5bELeWBDgvu1WcYYwqRlqtmI");
        }
    }
}
