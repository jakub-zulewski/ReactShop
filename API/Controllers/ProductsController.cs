using API.Data;
using API.Entities;
using API.Extensions;
using API.RequestHelpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class ProductsController(StoreContext storeContext) : BaseApiController
{
    private readonly StoreContext _storeContext = storeContext;

    [HttpGet]
    public async Task<ActionResult<PagedList<Product>>> GetProducts(
        [FromQuery] ProductParams productParams)
    {
        var query = _storeContext.Products
            .AsNoTracking()
            .Sort(productParams.OrderBy)
            .Search(productParams.SearchTerm)
            .Filter(productParams.Brands, productParams.Types)
            .AsQueryable();
        var products = await PagedList<Product>.ToPagedList(
            query, productParams.PageNumber, productParams.PageSize);

        Response.AddPaginationHeader(products.MetaData);

        return products;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _storeContext.Products.FindAsync(id);

        return product is null ? NotFound() : product;
    }
}