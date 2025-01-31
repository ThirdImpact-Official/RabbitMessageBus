using BuildingBlocks.Eventbus.Event;
using BuildingBlocks.Eventbus.EventBus;
using BuildingBlokcs.RabbitMQ.DependencyInjection;
using Subscriber.Event.EventHandler;
using Subscriber.ExtensionMethodes;
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
builder.Services.AddScoped<TestEventHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// méthodes for the bus to start
app.StartBus().GetAwaiter().GetResult();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
