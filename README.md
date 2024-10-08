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


```mermaid
flowchart TD
    subgraph ProductService["Product Service"]
        PS1[Create Product]
        PS2[Publish ProductCreated]
    end

    subgraph OrderService["Order Service"]
        OS1[Listen for ProductCreated]
        OS2[Create Order with Pending Status]
        OS3[Publish OrderPlaced]
        OS4[Listen for PaymentProcessed]
        OS5[Update Order Status to Completed]
    end

    subgraph PaymentService["Payment Service"]
        PS3[Listen for OrderPlaced]
        PS4[Process Payment]
        PS5[Publish PaymentProcessed]
    end

    subgraph AzureServiceBus["Azure Service Bus"]
        ProductCreatedTopic["ProductCreated"]
        OrderPlacedTopic["OrderPlaced"]
        PaymentProcessedTopic["PaymentProcessed"]
    end

    PS1 --> PS2
    PS2 --> ProductCreatedTopic
    ProductCreatedTopic --> OS1
    OS1 --> OS2
    OS2 --> OS3
    OS3 --> OrderPlacedTopic
    OrderPlacedTopic --> PS3
    PS3 --> PS4
    PS4 --> PS5
    PS5 --> PaymentProcessedTopic
    PaymentProcessedTopic --> OS4
    OS4 --> OS5

