using System.Text.Json.Serialization;

namespace EStoreX.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProductSortBy
    {
        Name,
        Price,
        OldPrice,
        Category,
        SalesCount
    }
}
