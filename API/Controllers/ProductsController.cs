using API.Data;
using API.Entities;
using API.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class ProductsController(StoreContext storeContext) : BaseApiController
{
    private readonly StoreContext _storeContext = storeContext;

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts(string orderBy)
    {
        var query = _storeContext.Products
            .AsNoTracking()
            .Sort(orderBy)
            .AsQueryable();

        return await query.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _storeContext.Products.FindAsync(id);

        return product is null ? NotFound() : product;
    }
}