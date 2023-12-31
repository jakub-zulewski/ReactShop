using API.Entities;
using API.Entities.OrderAggregate;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class StoreContext(DbContextOptions options) : IdentityDbContext<User, Role, int>(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasOne(x => x.Address)
            .WithOne()
            .HasForeignKey<UserAddress>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
