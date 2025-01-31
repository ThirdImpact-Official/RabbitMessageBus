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


