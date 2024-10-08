using Common.Dommain;
using ProductServices.CreateProduct;


namespace Tests.IntegrationTests;

public class FakePublisher : IPublisher
{
    public Task PublishProductCreated(Product product, CancellationToken token) => Task.CompletedTask;
}
