# 🐇 RabbitMessageBus  

RabbitMessageBus is a message bus implementation based on **RabbitMQ**, facilitating asynchronous communication between services.  

## 📌 Features  

👉 **Uses RabbitMQ**: Enables message transmission between services via RabbitMQ.  
👉 **Event-driven architecture**: Supports an event-based model for decoupled communication.  
👉 **Durable messages**: Allows configuration for message persistence.  
👉 **Scalability**: Designed for high scalability and efficient load handling.  
👉 **Flexibility**: Configurable queues and exchanges to adapt to various requirements.  

---  

## 🛠️ Prerequisites  

- **C# (.NET 8 or later)**  
- **RabbitMQ 7.0.0**  
- **Docker**  

---  

## 🚀 Explanation  

- **BuildingBlocks.Event** handles the definition of `IIntegrationEvent`, `IEventHandler`.  
- **BuildingBlocks.RabbitMQ** provides the implementation of the event bus using RabbitMQ.  

---  

## 📚 Tutorial  

### 🏠 Step 1: Creating an Event  

In both your **publisher service** and **consumer service**, create an event that inherits from `IntegrationEvent`:  

```csharp
public class TestEvent : IntegrationEvent
{
    public string Message { get; set; }
    public int EventId { get; set; }

    public TestEvent(string message, int eventId)
    {
        Message = message;
        EventId = eventId;
    }
}
```

---  

### 🔄 Step 2: Creating an Event Handler  

In your **consumer service**, add an event handler implementing `IEventHandler<T>`:  

```csharp
public class TestEventHandler : IEventHandler<TestEvent>
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<TestEventHandler> _logger;

    public TestEventHandler(IEventBus eventBus, ILogger<TestEventHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public Task Handle(TestEvent @event)
    {
        try
        {
            _logger.LogInformation("Test event received:");
            _logger.LogInformation($"Message: {@event.Message}");
            _logger.LogInformation($"Event ID: {@event.EventId}");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling event: {ex.Message}");
            throw;
        }
    }
}
```

---  

### ⚙️ Step 3: Dependency Injection in `Program.cs`  

In your **consumer service**, register the event bus and event handler:  

```csharp
// Add the RabbitMQ message bus
builder.Services.AddBuildinBlocksRabbitMQ(configuration);

// Register the event handler
builder.Services.AddScoped<IEventHandler<TestEvent>, TestEventHandler>();
```

---  

### 👀 Step 4: Subscribing to the Event in the Consumer Service  

In `Program.cs`, add the event subscription:  

```csharp
var eventBus = app.Services.GetRequiredService<IEventBus>();

await eventBus.StartAsync();

await eventBus.SubscribeAsync<TestEvent, TestEventHandler>(async e =>
    await new TestEventHandler(
        eventBus,
        app.Services.GetRequiredService<ILogger<TestEventHandler>>()
    ).Handle(e)
);
```

---  

## 🥾 Testing  

To test your implementation:  

1. **Start RabbitMQ** using Docker.  
2. **Run the publisher service** and publish an event.  
3. **Run the consumer service** and verify it receives and processes the event.  

---  

## 🎯 Conclusion  

You now have a fully functional message bus using **RabbitMQ** in .NET! 🎉  
If you have any questions or suggestions, feel free to contribute to the project. 🚀
