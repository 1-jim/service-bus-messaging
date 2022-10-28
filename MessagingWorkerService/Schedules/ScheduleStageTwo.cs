using Coravel.Invocable;
using Coravel.Queuing.Interfaces;
using MessagingWorkerService.Tasks;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingWorkerService.Schedules
{
    public class ScheduleStageTwo : IInvocable
    {
        private readonly IQueue _queue;
        private readonly IConfiguration _configuration;

        public ScheduleStageTwo(IQueue queue, IConfiguration configuration)
        {
            _queue = queue;
            _configuration = configuration;
        }

        public Task Invoke()
        {
            var request = Guid.NewGuid();
            _queue.QueueInvocableWithPayload<TaskStageTwo, Guid>(request);
            Log.Debug($"{request} Stage 2 Worker Scheduled for Execution");
            return Task.CompletedTask;
        }
    }
}
