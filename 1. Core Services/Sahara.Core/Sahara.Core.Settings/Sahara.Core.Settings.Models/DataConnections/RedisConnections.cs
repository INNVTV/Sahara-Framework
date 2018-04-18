using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Sahara.Core.Settings.Models.DataConnections
{
    public class RedisConnections
    {
        private string _redisUrl { get; set; }
        private string _redisKey { get; set; }

        //private string _platformManagerUrl { get; set; }
        //private string _platformManagerKey { get; set; }

        //private string _accountManagerUrl { get; set; }
        //private string _accountManagerKey { get; set; }

        //private string _accountDataUrl { get; set; }
        //private string _accountDataKey { get; set; }



        public RedisConnections(string redisUrl, string redisKey) //string platformManagerUrl, string platformManagerKey, string accountManagerUrl, string accountManagerKey)//, string accountDataUrl, string accountDataKey)
        {
            _redisUrl = redisUrl;
            _redisKey = redisKey;

            //_platformManagerUrl = platformManagerUrl;
            //_platformManagerKey = platformManagerKey;

            //_accountManagerUrl = accountManagerUrl;
            //_accountManagerKey = accountManagerKey;

            //_accountDataUrl = accountDataUrl;
            //_accountDataKey = accountDataKey;
        }

        public ConfigurationOptions RedisConfiguration
        {
            get
            {
                var _configuration = ConfigurationOptions.Parse(_generateConnectionString(_redisUrl, _redisKey));
                _configuration.AllowAdmin = true;
                return _configuration;
            }
        }

        /*
        public ConfigurationOptions PlatformManager_RedisConfiguration
        {
            get
            {
                var _configuration = ConfigurationOptions.Parse(_generateConnectionString(_platformManagerUrl, _platformManagerKey));
                _configuration.AllowAdmin = true;
                return _configuration;
            }
        }

        public ConfigurationOptions AccountManager_RedisConfiguration
        {
            get
            {
                var _configuration = ConfigurationOptions.Parse(_generateConnectionString(_accountManagerUrl, _accountManagerKey));
                _configuration.AllowAdmin = true;
                return _configuration;
            }
        }

        
        public ConfigurationOptions AccountData_RedisConfiguration
        {
            get
            {
                var _configuration = ConfigurationOptions.Parse(_generateConnectionString(_accountDataUrl, _accountDataKey));

                return _configuration;
            }
        }
        */


        private string _generateConnectionString(string url, string key)
        {
            StringBuilder connectionString = new StringBuilder();

            connectionString.Append(url);
            connectionString.Append(", ssl=false, password=");
            connectionString.Append(key);

            return connectionString.ToString();

        }

    }

    public class RedisConnectionMultiplexers
    {
        private ConnectionMultiplexer _redisMultiplexer { get; set; }

        //private ConnectionMultiplexer _platformManager_Multiplexer { get; set; }

        //private ConnectionMultiplexer _accountManager_Multiplexer { get; set; }



        public RedisConnectionMultiplexers(ConnectionMultiplexer redisMultiplexer)//ConnectionMultiplexer platformManager_Multiplexer, ConnectionMultiplexer accountManager_Multiplexer)
        {
            _redisMultiplexer = redisMultiplexer;
            //_platformManager_Multiplexer = platformManager_Multiplexer;
            //_accountManager_Multiplexer = accountManager_Multiplexer;
        }


        public ConnectionMultiplexer RedisMultiplexer
        {
            get
            {
                return _redisMultiplexer;
            }
        }

        /*
        public ConnectionMultiplexer PlatformManager_Multiplexer
        {
            get
            {
                return _platformManager_Multiplexer;
            }
        }

        public ConnectionMultiplexer AccountManager_Multiplexer
        {
            get
            {
                return _accountManager_Multiplexer;
            }
        }
        */

    }
}
