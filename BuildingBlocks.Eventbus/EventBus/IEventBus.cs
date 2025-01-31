using BuildingBlocks.Eventbus.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Eventbus.EventBus
{
    public interface IEventBus : IAsyncDisposable
    {
        /// <summary>
        /// Publishes an event to the bus.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event to publish.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent;

        /// <summary>
        /// Subscribes to an event on the bus.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <typeparam name="THandler">The type of the event handler.</typeparam>
        /// <param name="handler">The event handler delegate.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SubscribeAsync<TEvent, THandler>(Func<TEvent, Task> handler, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
            where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// Unsubscribes from an event on the bus.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <typeparam name="THandler">The type of the event handler.</typeparam>
        /// <param name="handler">The event handler delegate to remove.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UnsubscribeAsync<TEvent, THandler>(Func<TEvent, Task> handler, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
            where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// Starts the event bus (e.g., establishes a connection to the message broker).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}
