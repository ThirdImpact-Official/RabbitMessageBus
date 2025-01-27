using Sender.Command;

var endpoint= Endpoint.Create("sender");

endpoint.Start();

var command = new CreateOrderCommand(1,"Huh huh Cat");

endpoint.Send(command,"ordering");

var @event = new CustomerEmailChanged(1,"HuhCat@gmail.com");

endpoint.Publish(@event);
