# 🐇 RabbitMessageBus

RabbitMessageBus est une implémentation d'un bus de messages basé sur **RabbitMQ**, facilitant la communication asynchrone entre services.

## 📌 Caractéristiques

✅ **Utilisation de RabbitMQ** : Transmission de messages entre services via RabbitMQ.  
✅ **Gestion des événements** : Prise en charge d'un modèle d'événements pour une architecture découplée.  
✅ **Messages durables** : Possibilité de configurer la persistance des messages.  
✅ **Scalabilité** : Adapté aux applications évolutives avec une montée en charge efficace.  
✅ **Flexibilité** : Paramétrage des files d'attente et des échanges pour s'adapter aux besoins.  

---

## 🛠️ Prérequis

- **C# (.NET 8 ou supérieur)**
- **RabbitMQ 7.0.0**
- **Docker**

---

## 🚀 Explication

- BuildingBloks.Event s'occupe de la définition des interface IIntegrationEvent,IEventHandler 

- BuildongBlocks.RabbitMQ de l'implementation du bus d'evenement avec rabbitMQ
---

## 📖 Tutoriel

### 🏗️ Étape 1 : Création d'un événement  

Dans votre **service publisher** et **service consumer**, créez un événement qui hérite de `IntegrationEvent` :

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

### 🔄 Étape 2 : Création d'un handler pour l'événement  

Dans votre **service consumer**, ajoutez un gestionnaire d'événement qui implémente `IEventHandler<T>` :

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

### ⚙️ Étape 3 : Injection des dépendances dans `Program.cs`  

Dans votre **service consumer**, enregistrez le bus d'événements et le gestionnaire d'événements :

```csharp
// Ajout du bus de messages RabbitMQ
builder.Services.AddBuildinBlocksRabbitMQ(configuration);

// Enregistrement du handler de l'événement
builder.Services.AddScoped<TestEventHandler>();
```
dans l'appsetting.json
```json
    "EventBus": {
      "HostName": "localhost",
      "UserNames": "guest",
      "Password": "guest"
    }
```
---

### 📡 Étape 4 : Souscription à l'événement dans le service consumer  

Dans `Program.cs`, ajoutez l'abonnement à l'événement :

```csharp
var eventBus = app.Services.GetRequiredService<IEventBus>();

await eventBus.StartAsync();

await eventBus.SubscribeAsync<TestEvent, TestEventHandler>();
```

---

## 🧪 Test  

Pour tester votre implémentation :

1. **Démarrez RabbitMQ** avec Docker.
2. **Lancez le service publisher** et publiez un événement.
3. **Lancez le service consumer** et vérifiez qu'il reçoit et traite l'événement.

---

## 🎯 Conclusion  

Vous avez maintenant un bus de messages fonctionnel basé sur **RabbitMQ** en .NET ! 🎉  
Si vous avez des questions ou suggestions, n'hésitez pas à contribuer au projet. 🚀

