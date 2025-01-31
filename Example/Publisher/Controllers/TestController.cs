using BuildingBlocks.Eventbus.EventBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Publisher.IntegrationEvents;
using System.ComponentModel.Design;

namespace Publisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEventBus eventbus;
        private ILogger<TestController> logger;

        public TestController(IEventBus eventbus, ILogger<TestController> logger)
        {
            this.eventbus = eventbus;
            this.logger = logger;
        }
        /// <summary>
        /// allow tou to test the event from publisher 
        /// and Send it to the Subscriber 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> TsestEventFromPublisher()
        {
            try
            {
                //creation of the event
                TestEvent testEvent = new TestEvent("test", 1);
                //send the event to the eventbuss
                await eventbus.PublishAsync(testEvent, CancellationToken.None);

                return Ok();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
