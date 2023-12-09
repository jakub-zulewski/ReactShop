using API.Data;
using API.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(StoreContext storeContext) : ControllerBase
{
    private readonly StoreContext _storeContext = storeContext;

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
        => await _storeContext.Products.ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
        => await _storeContext.Products.FindAsync(id);
}