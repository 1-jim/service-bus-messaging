using Azure.Messaging.ServiceBus;

namespace MessagingWorkerService.Services
{
    public interface IBusService
    {
        Task<List<ServiceBusReceivedMessage>> PeekMessagesAsync(int batchSize = 20);
        Task<string> SendMessageAsync(string messageId, string messageBody);
    }
}