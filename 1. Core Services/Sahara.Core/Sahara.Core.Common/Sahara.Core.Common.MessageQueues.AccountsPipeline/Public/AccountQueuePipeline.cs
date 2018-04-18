using System;
using Microsoft.WindowsAzure.Storage.Queue;
using Sahara.Core.Common.MessageQueues.AccountsPipeline.PublicTypes;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Common.MessageQueues.AccountsPipeline
{
    public static class AccountQueuePipeline
    {
        /* UNUSED
        #region Constructor

        private static CloudQueue _queue = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudQueueClient().GetQueueReference(PipelineSettings.QueueReferenceName);

        static AccountQueuePipeline()
        {
            //Create and set retry policy
            IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 8);
            _queue.ServiceClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

            _queue.CreateIfNotExists();
        }

        #endregion

        #region Send

        /// <summary>
        /// Usage:
        /// AccountQueuePipeline.SendMessage.MethodName("param1", "param2");
        /// </summary>
        public static class SendMessage
        {

            public static void MessageOne(string accountId, string userId)
            {
                string message = string.Format("{0},{1}", AccountMessageTypes.MessageOne, accountId, userId);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);

            }

            public static void AnotherMethod(string accountId, string userId, string property1, string property2, string property3)
            {
                string message = string.Format("{0},{1},{2},{3},{4},{5}", AccountMessageTypes.AnotherMessage, accountId, userId, property1, property2, property3);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);
            }
        }

        #endregion

        #region Get

        public static AccountQueueMessage GetNextQueueMessage(TimeSpan timeSpan)
        {
            var message = _queue.GetMessage(timeSpan);

            string messageType;
            string[] messageParts;

            if (message != null)
            {
                messageParts = message.AsString.Split(new char[] { ',' });
                messageType = messageParts[0];
            }
            else
            {
                return null;
            }


            switch (messageType)
            {
                case AccountMessageTypes.MessageOne:
                    MessageOne_QueueMessage MessageOne = new MessageOne_QueueMessage();
                    MessageOne.rawMessage = message;
                    MessageOne.MessageType = messageType;
                    MessageOne.AccountID = messageParts[1];
                    MessageOne.UserID = messageParts[2];
                    return MessageOne;


                case AccountMessageTypes.AnotherMessage:
                    AnotherMessage_QueueMessage anotherMessage = new AnotherMessage_QueueMessage();
                    anotherMessage.rawMessage = message;
                    anotherMessage.MessageType = messageType;
                    anotherMessage.AccountID = messageParts[1];
                    anotherMessage.UserID = messageParts[2];
                    anotherMessage.Property1 = messageParts[3];
                    anotherMessage.Property2 = messageParts[4];
                    anotherMessage.Property3 = messageParts[5];
                    return anotherMessage;

                default:
                    return null;

            }


            //return queueMessage;

        }

        #endregion

        #region Delete

        public static bool DeleteQueueMessage(AccountQueueMessage queueMessage)
        {
            _queue.DeleteMessage(queueMessage.rawMessage);
            return true;
        }

        #endregion
        */



    }
}
