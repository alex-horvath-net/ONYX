# ONYX

```mermaid
sequenceDiagram
    participant ProductService
    participant ServiceBus as Azure Service Bus
    participant OrderService
    participant PaymentService

    ProductService->>ProductService: Create Product
    ProductService->>ServiceBus: Publish ProductCreated
    ServiceBus->>OrderService: ProductCreated Event Received
    OrderService->>OrderService: Create Order (Pending)
    OrderService->>ServiceBus: Publish OrderPlaced
    ServiceBus->>PaymentService: OrderPlaced Event Received
    PaymentService->>PaymentService: Process Payment
    PaymentService->>ServiceBus: Publish PaymentProcessed
    ServiceBus->>OrderService: PaymentProcessed Event Received
    OrderService->>OrderService: Update Order Status to Completed


