using System.Net.Http.Headers;

namespace IntegrationTests.TestHelpers;

public static class HttpRequestFactory
{
    public static HttpRequestMessage Post(string url) => new(HttpMethod.Post, url);

    public static HttpRequestMessage WithCookie(this HttpRequestMessage request, string name, string value)
    {
        request.Headers.Add("Cookie", $"{name}={value}");
        return request;
    }

    public static HttpRequestMessage WithBearerToken(this HttpRequestMessage request, string accessToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    public static string ExtractRefreshToken(HttpResponseMessage response)
    {
        var setCookie = response.Headers.GetValues("Set-Cookie").First();
        const string prefix = "refreshToken=";
        var start = setCookie.IndexOf(prefix) + prefix.Length;
        var end = setCookie.IndexOf(';', start);
        return end > start ? setCookie[start..end] : setCookie[start..];
    }
}
