using Azure.Messaging.ServiceBus;
using MessagingWorkerService.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingWorkerService.Services
{
    public class BusService : IBusService
    {
        private readonly BusConfiguration _configuration;

        public BusService(BusConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<ServiceBusReceivedMessage>> PeekMessagesAsync(int batchSize = 20)
        {
            var client = new ServiceBusClient(_configuration.Connection);
            var receiver = client.CreateReceiver(_configuration.Queue);

            var messages = new List<ServiceBusReceivedMessage>();
            var sequenceNumber = 0L;
            try
            {
                do
                {
                    var batch = await receiver.PeekMessagesAsync(batchSize, sequenceNumber);
                    if (batch.Count <= 0)
                    {
                        break;
                    }
                    sequenceNumber = batch[^1].SequenceNumber + 1;
                    messages.AddRange(batch);
                } while (true);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service Bus Read Failure");
            }

            return messages;
        }

        public async Task<string> SendMessageAsync(string messageId, string messageBody)
        {
            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            var client = new ServiceBusClient(_configuration.Connection, options: clientOptions);
            var sender = client.CreateSender(_configuration.Queue);

            using ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync();
            if (!batch.TryAddMessage(new ServiceBusMessage(messageBody)))
            {
                throw new Exception($"The message id {messageId} is too large!");
            }

            try
            {
                await sender.SendMessagesAsync(batch);
                Log.Debug($"Message {messageId} was published to {_configuration.Queue}");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service Bus Send Failure");
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            return messageId;
        }
    }
}
