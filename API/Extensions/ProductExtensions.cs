using API.Entities;

namespace API.Extensions;

public static class ProductExtensions
{
    public static IQueryable<Product> Sort(this IQueryable<Product> query, string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderBy(x => x.Name);

        query = orderBy switch
        {
            "price" => query.OrderBy(x => x.Price),
            "priceDesc" => query.OrderByDescending(x => x.Price),
            _ => query.OrderBy(x => x.Name)
        };

        return query;
    }
}
