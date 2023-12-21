using System.Text.Json;

using API.Configs;
using API.RequestHelpers;

namespace API.Extensions;

public static class HttpExtensions
{
    public static void AddPaginationHeader(this HttpResponse response, MetaData metaData)
    {
        response.Headers.Append("Pagination", JsonSerializer.Serialize(
            metaData, JsonConfig.JsonSerializerOptionsCamelCase));

        response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
    }
}
