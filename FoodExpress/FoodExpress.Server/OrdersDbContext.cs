using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Server
{
    public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
    {
        public DbSet<OrderEntity> Orders => Set<OrderEntity>();
        public DbSet<SubscriptionEntity> Subscriptions => Set<SubscriptionEntity>();

    }

    public class OrderEntity
    {
        public Guid Id { get; set; }
        public int RestaurantId { get; set; }
        public string ItemsJson { get; set; } = "[]";
        public string Status { get; set; } = "Placed";
    }

    public class SubscriptionEntity
    {
        public Guid Id { get; set; }
        public string PlanName { get; set; } = "Monthly";
        public string Status { get; set; } = "Active";
    }
}
