using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Settings.Models.DataConnections;
using StackExchange.Redis;

namespace Sahara.Core.Settings.Azure
{
    public static class Redis
    {

        #region Private Properties

        private static RedisConnections _redisConnections;

        //Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers (per StackExchange.Redis documentation)
        public static RedisConnectionMultiplexers _redisConnectionMultiplexers;


        #endregion

        #region INITIALIZATION

        internal static void Initialize()
        {
            #region Initialize Redis Connections

            switch (Environment.Current.ToLower())
            {


                #region Production

                case "production":
                    _redisConnections = new RedisConnections(

                        //Global (C1 Dedicated (or C2 Dedicated ???)
                        //IMPORTANT SETTINGS on Azure: Allow access only via SSL = OFF !!!!!
                        "[Config_Name].redis.cache.windows.net",
                        "[Config_Key]"

                        );
                    break;

                #endregion


                #region Stage

                case "stage":
                    _redisConnections = new RedisConnections(

                        //Global (C0 Shared)
                        //IMPORTANT SETTINGS on Azure: Allow access only via SSL = OFF !!!!!
                        "[Config_Name]-Stage.redis.cache.windows.net",
                        "[Config_Key]"

                        );
                    break;


                #endregion


                #region Local/Debug

                case "debug":
                    _redisConnections = new RedisConnections(

                        //Global
                        //IMPORTANT SETTINGS on Azure: Allow access only via SSL = OFF !!!!!
                        "[Config_Name]-Debug.redis.cache.windows.net",
                        "[Config_Key]"

                        );
                    break;


                case "local":
                    _redisConnections = new RedisConnections(

                        //Global (C1 Dedicated (or C2 Dedicated ???)
                        //IMPORTANT SETTINGS on Azure: Allow access only via SSL = OFF !!!!!
                        "[Config_Name]-Local.redis.cache.windows.net",
                        "[Config_Key]"

                        );
                    break;

                #endregion

                default:
                    _redisConnections = null;
                    break;


            }

            #endregion


            #region Initialize Multiplexers

            //Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers.
            //You should not create a  ConnectionMultiplexer  per operation. It is fully thread-safe and ready for this usage.
            //In all the subsequent examples it will be assumed that you have a  ConnectionMultiplexer  instance stored away for re-use.

            _redisConnectionMultiplexers = new RedisConnectionMultiplexers(

                ConnectionMultiplexer.Connect(RedisConnections.RedisConfiguration)
                );


            #endregion

        }

        #endregion


        private static RedisConnections RedisConnections
        {
            get
            {
                return _redisConnections;
            }
        }


        public static RedisConnectionMultiplexers RedisMultiplexers
        {
            //Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers.
            //You should not create a  ConnectionMultiplexer  per operation. It is fully thread-safe and ready for this usage.
            //In all the subsequent examples it will be assumed that you have a  ConnectionMultiplexer  instance stored away for re-use.

            get
            {
                return _redisConnectionMultiplexers;
            }
        }
        

    }
}
