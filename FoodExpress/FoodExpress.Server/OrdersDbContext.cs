using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Server
{
    public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
    {
        public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    }

    public class OrderEntity
    {
        public Guid Id { get; set; }
        public int RestaurantId { get; set; }
        public string ItemsJson { get; set; } = "[]";
        public string Status { get; set; } = "Placed";
    }
}
