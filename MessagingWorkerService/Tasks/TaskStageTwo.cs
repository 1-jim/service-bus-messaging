using Coravel.Invocable;
using Newtonsoft.Json;
using Serilog;

namespace MessagingWorkerService.Tasks
{
    public class TaskStageTwo :IInvocable, IInvocableWithPayload<Guid>
    {
        private readonly Services.IBusService _busService;
        private readonly Services.IOutputFileService _outputFileService;

        public TaskStageTwo(Services.IBusService busService, Services.IOutputFileService outputFileService)
        {
            _busService = busService;
            _outputFileService = outputFileService;
        }

        public Guid Payload { get; set; }

        public async Task Invoke()
        {
            var messages = await _busService.PeekMessagesAsync(50);
            foreach (var message in messages)
            {
                var body = message.Body.ToString();
                _outputFileService.CreateCsvOutput(body, "test");
                _outputFileService.CreateTempTimestampedJsonFile(body, message.MessageId);
            }
            Log.Debug($"{Payload} Task Two Completed");
        }
    }
}
