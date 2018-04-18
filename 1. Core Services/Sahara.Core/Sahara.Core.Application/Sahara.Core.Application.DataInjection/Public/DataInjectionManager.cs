using Sahara.Core.Common.MessageQueues.PlatformPipeline;
using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Testing.Public
{
    public static class DataInjectionManager
    {
        /// <summary>
        /// We send this job to the worker via Queue for background prcessing
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="imageDocumentInjectionCount"></param>
        /// <returns></returns>
        public static DataAccessResponseType InjectDocuments(string accountId, int imageDocumentInjectionCount)
        {

            var result = PlatformQueuePipeline.SendMessage.SendApplicationDataInjectionImageDocuments(accountId, imageDocumentInjectionCount);

            if(result)
            {
                return new DataAccessResponseType { isSuccess = true, SuccessMessage = "Data injection job has been placed into the queue. Worker role is currently operating in sequences of " + Sahara.Core.Settings.Platform.Worker.EasebackRound3.duration + ". Check the data injection log for processing results within that time frame." };
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, SuccessMessage = "Could not submit task into queue. Please try again." };
            }
        }
    }
}
