# RabbitMessageBus

RabbitMessageBus est une implémentation d'un bus de messages utilisant RabbitMQ.

## Caractéristiques

- Implémentation d'un bus de messages RabbitMQ fonctionnel.
- Exemples de code pour l'utilisation.

## Prérequis

- **C#**
- **Docker**

## Installation

1. Clonez le dépôt :

   ```bash
   git clone https://github.com/ThirdImpact-Official/RabbitMessageBus.git
   ```

2. Accédez au dossier du projet :

   ```bash
   cd RabbitMessageBus
   ```

3. Construisez l'image Docker de rabbit mq :

   ```bash
   docker-compose up rabbitmq .
   ```
## Principales Fonctionnalités
Intégration avec RabbitMQ :

Utilise RabbitMQ comme système de messagerie sous-jacent pour la transmission de messages.
Envoi et Réception de Messages :

Permet l'envoi et la réception de messages entre différents services.
Gestion des Événements :

Supporte la gestion des événements pour faciliter la communication asynchrone.
Exemples de Code :

Fournit des exemples pratiques pour aider les développeurs à intégrer et utiliser le bus de messages facilement.
Support pour les Messages Durables :

Permet de configurer des messages durables pour assurer la persistance des données.
Scalabilité :

Conçu pour être scalable, permettant de gérer une augmentation du volume de messages.
Flexibilité :

Offre une flexibilité dans la configuration des échanges et des files d'attente.
## tutorrial 

étapes 1 
vous devez d'abords creer un event dans votre service Publisher et dans votre service consommateur
celui-ci  doit impérativement hérité de Integration event 
```cs
    public class TestEvent :IntegrationEvent
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

étapes 2 

vous devez creer un handler capable de gérer l'evennement  

```cs
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
              Console.WriteLine("Test event handled");
              Console.WriteLine(@event.Message);
              Console.WriteLine(@event.Id);
              Console.WriteLine("j'ai réussi grosse pute");
              return Task.CompletedTask;

          }
          catch (Exception)
          {

              throw;
          }
      }
```

étapes 3

dans votre program.cs vous devez injecter le bus ainsi que handler
```cs
// previous contentn ......
 
builder.Services.AddBuildinBlocksRabbitMQ(configuration);
builder.Services.AddScoped<IEventHandler<TestEvent>, TestEventHandler>();

//......
```

étape 4 
subscribe the event  to the bus in the consummer service 
```cs
//-------var event bus -----------
var eventBus = app.Services.GetRequiredService<IEventBus>(); //b
await eventBus.StartAsync();

await eventBus.SubscribeAsync<TestEvent, TestEventHandler>(async e => await new TestEventHandler(eventBus,app.Services.GetRequiredService<ILogger<TestEventHandler>>()).Handle(e));

```
