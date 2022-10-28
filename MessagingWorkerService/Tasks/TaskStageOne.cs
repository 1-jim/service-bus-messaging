using Coravel.Invocable;
using MessagingWorkerService.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingWorkerService.Tasks
{
    public class TaskStageOne :IInvocable, IInvocableWithPayload<Guid>
    {
        private readonly Services.IBusService _busService;
        public TaskStageOne(Services.IBusService busService)
        {
            _busService = busService;
        }

        public Guid Payload { get; set; }

        public async Task Invoke()
        {
            var id = Guid.NewGuid().ToString();
            MessageModel messageModel = new()
            {
                Id = Guid.Parse(id),
                Message = $"New Message {id}",
                Meta = new()
                {
                    Stuff = $"stuff {id}",
                    Things = $"things {id}"
                }
            };
            var message = JsonConvert.SerializeObject(messageModel);

            await _busService.SendMessageAsync(id, message); 
        }
    }
}
