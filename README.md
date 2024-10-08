# ONYX

```mermaid
sequenceDiagram
    participant ProductService
    participant OrderService
    participant PaymentService

    ProductService->>ProductService: Create Product
    ProductService->>OrderService: Publish ProductCreated
    OrderService->>OrderService: Create Order (Pending)
    OrderService->>PaymentService: Publish OrderPlaced
    PaymentService->>PaymentService: Process Payment
    PaymentService->>OrderService: Publish PaymentProcessed
    OrderService->>OrderService: Update Order Status to Completed
