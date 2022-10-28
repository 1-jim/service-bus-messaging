using Coravel.Invocable;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingWorkerService.Tasks
{
    public class TaskStageTwo :IInvocable, IInvocableWithPayload<Guid>
    {
        private readonly Services.IBusService _busService;
        public TaskStageTwo(Services.IBusService busService)
        {
            _busService = busService;
        }

        public Guid Payload { get; set; }

        public async Task Invoke()
        {
            var messages = await _busService.PeekMessagesAsync(50);
            foreach (var message in messages)
            {
                string body = message.Body.ToString();
                //convert to model
            }
            Log.Debug($"{Payload} Task One Completed");
        }
    }
}
