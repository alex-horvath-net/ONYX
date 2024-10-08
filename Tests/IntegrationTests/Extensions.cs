using Common.Infrastucture.Data;

namespace Tests.IntegrationTests {
    public static class Extensions {
        public static void Seed(this AppDB db) {
            db.Products.Add(new() { Name = "Test Product 1", Colour = "Red", Price = 100 });
            db.Products.Add(new() { Name = "Test Product 2", Colour = "Green", Price = 200 });
            db.SaveChanges();
        }
    }
}
