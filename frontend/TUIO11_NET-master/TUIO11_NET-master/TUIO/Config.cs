using Newtonsoft.Json;  // Use this for deserialization
using System;
using System.IO;

namespace TUIO
{
    public class Config
    {
        public ServerConfig server { get; set; }
        public ClientConfig client { get; set; }
        public string FIREBASE_SERVICE_ACCOUNT { get; set; }

        public class ServerConfig
        {
            public string IP { get; set; }
            public int port { get; set; }
        }

        public class ClientConfig
        {
            public int reconnectTimeout { get; set; }  // This is the timeout value you want to use
            public int maxRetries { get; set; }
        }

        public static Config ReadConfig(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(json);
        }
    }

}
