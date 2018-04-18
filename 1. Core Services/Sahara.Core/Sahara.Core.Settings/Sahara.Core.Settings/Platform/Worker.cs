using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Platform
{
    public static class Worker
    {

        #region BASE SETTINGS

        /// <summary>
        /// Amount of time worker sleeps between checking the queue for messages
        /// </summary>
        public static class TaskBreak
        {
            public static int milliseconds;
            public static string duration = null;
        }

        public static class EasebackRound1
        {
            public static int emptyCacheCountMax;
            public static int milliseconds;
            public static string duration = null;
        }

        public static class EasebackRound2
        {
            public static int emptyCacheCountMax;
            public static int milliseconds;
            public static string duration = null;
        }

        public static class EasebackRound3
        {
            public static int milliseconds;
            public static string duration = null;
        }

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Environment Settings

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    //Sleep time between each each queue check
                    TaskBreak.milliseconds = 2000;
                    TaskBreak.duration = "2 Seconds";

                    // Check queue every 36 seconds for first 6 minutes while empty
                    EasebackRound1.emptyCacheCountMax = 10;
                    EasebackRound1.milliseconds = 30000;
                    EasebackRound1.duration = "30 Seconds";

                    // Then check queue every 2 minutes for 40 minutes  while empty
                    EasebackRound2.emptyCacheCountMax = 20;
                    EasebackRound2.milliseconds = 120000;
                    EasebackRound2.duration = "2 Minutes";

                    // Then check queue every 4 minutes indefinetly until the queue has a message to process
                    EasebackRound3.milliseconds = 240000; 
                    EasebackRound3.duration = "4 Minutes";

                    break;

                #endregion


                #region Stage

                case "stage":

                    //Sleep time between each each queue check
                    TaskBreak.milliseconds = 3000;
                    TaskBreak.duration = "3 Seconds";

                    // Check queue every 36 seconds for first 6 minutes while empty
                    EasebackRound1.emptyCacheCountMax = 10;
                    EasebackRound1.milliseconds = 45000;
                    EasebackRound1.duration = "45 Seconds";

                    // Then check queue every 2 minutes for 40 minutes  while empty
                    EasebackRound2.emptyCacheCountMax = 20;
                    EasebackRound2.milliseconds = 120000;
                    EasebackRound2.duration = "2 Minutes";

                    // Then check queue every 4 minutes indefinetly until the queue has a message to process
                    EasebackRound3.milliseconds = 240000;
                    EasebackRound3.duration = "4 Minutes";

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    //Sleep time between each each queue check
                    TaskBreak.milliseconds = 15000;
                    TaskBreak.duration = "15 Seconds";

                    // Check queue every 45 seconds for first 7.5 minutes while empty
                    EasebackRound1.emptyCacheCountMax = 10;
                    EasebackRound1.milliseconds = 45000;
                    EasebackRound1.duration = "45 Seconds";

                    // Then check queue every 3 minutes for 60 minutes  while empty
                    EasebackRound2.emptyCacheCountMax = 20;
                    EasebackRound2.milliseconds = 180000;
                    EasebackRound2.duration = "3 Minutes";

                    // Then check queue every 40 minutes indefinetly until the queue has a message to process
                    EasebackRound3.milliseconds = 1800000;
                    EasebackRound3.duration = "30 Minutes";

                    break;

                case "local":

                    //Sleep time between each each queue check
                    TaskBreak.milliseconds = 15000;
                    TaskBreak.duration = "15 Seconds";

                    // Check queue every 45 seconds for first 7.5 minutes while empty
                    EasebackRound1.emptyCacheCountMax = 10;
                    EasebackRound1.milliseconds = 45000;
                    EasebackRound1.duration = "45 Seconds";

                    // Then check queue every 3 minutes for 60 minutes  while empty
                    EasebackRound2.emptyCacheCountMax = 20;
                    EasebackRound2.milliseconds = 180000;
                    EasebackRound2.duration = "3 Minutes";

                    // Then check queue every 40 minutes indefinetly until the queue has a message to process
                    EasebackRound3.milliseconds = 1800000;
                    EasebackRound3.duration = "30 Minutes";

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
