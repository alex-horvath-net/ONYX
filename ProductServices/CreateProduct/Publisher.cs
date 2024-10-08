using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Dommain;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ProductServices.CreateProduct;
public class Publisher(ServiceBusClient client) : IPublisher {
    public async Task PublishProductCreated(Product product, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        var eventContent = new PruductCreated(product.Id);
        var messageBody = JsonSerializer.Serialize(eventContent);
        var message = new ServiceBusMessage(messageBody);
        var sender = client.CreateSender("pruduct-created");
        await sender.SendMessageAsync(message, token);
    }
}

public class ResilientPublisher : IPublisher {
    private readonly IPublisher publisher;
    private readonly ILogger<ResilientPublisher> logger;
    private readonly AsyncRetryPolicy retryPolicy;
    private readonly AsyncCircuitBreakerPolicy circuitBreakerPolicy;

    public ResilientPublisher(IPublisher publisher, ILogger<ResilientPublisher> logger) {
        this.publisher = publisher;
        this.logger = logger;
        retryPolicy = Policy
            .Handle<ServiceBusException>(ex => ex.IsTransient)                                  // Retry on transient ServiceBus exceptions
            .Or<ServiceBusException>(ex => ex.Reason == ServiceBusFailureReason.ServiceBusy)    // Retry on transient ServiceBus exceptions
            .Or<TimeoutException>()                                                             // Retry on timeouts
            .WaitAndRetryAsync(
               retryCount: 5,
               retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
               onRetry: (exception, timeSpan, retryCount) => logger.LogDebug($"Retrying due to: {exception.Message}. This is retry {retryCount}"));

        // Define the circuit breaker policy: break for 30 seconds after 2 consecutive exceptions
        circuitBreakerPolicy = Policy
            .Handle<ServiceBusException>(ex => ex.IsTransient)
            .Or<ServiceBusException>(ex => ex.Reason == ServiceBusFailureReason.ServiceBusy)
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
               exceptionsAllowedBeforeBreaking: 3,
               durationOfBreak: TimeSpan.FromSeconds(30),
               onBreak: (exception, duration) => logger.LogDebug($"Circuit breaker opened due to {exception.Message} for {duration.TotalSeconds} seconds."),
               onReset: () => logger.LogDebug("Circuit breaker reset, resuming normal operation."));

    }

    public Task PublishProductCreated(Product product, CancellationToken token) =>
        retryPolicy.ExecuteAsync(() => circuitBreakerPolicy.ExecuteAsync(() =>
            publisher.PublishProductCreated(product, token)));
}

public record PruductCreated(int Id);
