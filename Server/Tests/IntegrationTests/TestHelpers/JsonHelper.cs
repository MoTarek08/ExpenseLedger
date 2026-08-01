using System.Text;
using System.Text.Json;

namespace IntegrationTests.TestHelpers;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static StringContent Serialize<T>(T value)
        => new(JsonSerializer.Serialize(value, Options), Encoding.UTF8, "application/json");
}
