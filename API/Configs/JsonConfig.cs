using System.Text.Json;

namespace API.Configs;

public static class JsonConfig
{
    public static readonly JsonSerializerOptions JsonSerializerOptionsCamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
