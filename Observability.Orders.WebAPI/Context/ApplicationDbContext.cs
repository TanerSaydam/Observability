using Microsoft.EntityFrameworkCore;
using Observability.Orders.WebAPI.Models;

namespace Observability.Orders.WebAPI.Context;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
