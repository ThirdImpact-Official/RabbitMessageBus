using BuildingBlocks.Eventbus.Event;
using BuildingBlocks.Eventbus.EventBus;
using BuildingBlokcs.RabbitMQ.DependencyInjection;
using Subscriber.Event.EventHandler;
using Subscriber.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Add the depedncy for the event bus 
builder.Services.AddBuildingBlocksRabbitMQ(builder.Configuration);
//add The Evfent handler as a service
builder.Services.AddScoped<IEventHandler<TestEvent>,TestEventHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//add the vent and the suvscription 
var eventBus = app.Services.GetRequiredService<IEventBus>(); //b
//start the connection to the bus 
await eventBus.StartAsync();
//subscription to the event bus
await eventBus.SubscribeAsync<TestEvent, TestEventHandler>(async e => await new TestEventHandler(eventBus, app.Services.GetRequiredService<ILogger<TestEventHandler>>()).Handle(e));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
