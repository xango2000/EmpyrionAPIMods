using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace EntityOwnership
{
    class YamlReaders
    {
        public static YamlData Retrieve(String filePath)
        {
            var input = System.IO.File.OpenText(filePath);
            var deserializer = new Deserializer();
            var Contacts = deserializer.Deserialize<YamlData>(input);
            return Contacts;
        }
        public class YamlData
        {
            public ServerConfig ServerConfig { get; set; }
            public GameConfig GameConfig { get; set; }
        }
        public class ServerConfig
        {
            public int Srv_Port { get; set; }
            public string Srv_Name { get; set; }
            public string Srv_Password { get; set; }
            public int Srv_MaxPlayers { get; set; }
            public string SaveDirectory { get; set; }
        }
        public class GameConfig
        {
            public string GameName { get; set; }
            public string Name { get; set; }
            public int Seed { get; set; }
            public string CustomScenario { get; set; }
        }

    }
}
