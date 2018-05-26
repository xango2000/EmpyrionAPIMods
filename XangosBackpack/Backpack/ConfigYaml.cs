using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;


namespace VirtualBackpack
{
    class ConfigYaml
    {
        public class Root
        {
            public int DefaultCountVirtualBackpacks { get; set; }
            public int DefaultCountToolbars { get; set; }
            public int DefaultCountBackpacks { get; set; }
            public List<Playerdata> PlayerData { get; set; }
        }
        public class Playerdata
        {
            public string PlayerName { get; set; }
            public int SteamID { get; set; }
            public int MaxVirtualBackpacks { get; set; }
            public int MaxToolbars { get; set; }
            public int MaxBackpacks { get; set; }
            public List<Names> VirtualBackpacks { get; set; }
            public List<Names> Toolbars { get; set; }
            public List<Names> Backpacks { get; set; }
        }
        public class Names
        {
            public string Name { get; set; }
        }
        /*
        public static ConfigYaml Retrieve(String filePath)
        {
            var input = File.OpenText(filePath);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            return deserializer.Deserialize<ConfigYaml>(input);
        }
        */
        public static Root Retrieve(String filePath)
        {
            using (StreamReader input = File.OpenText(filePath))
            {
                return (new Deserializer()).Deserialize<Root> (input);
            }
            /*
            var input = File.OpenText(filePath);
            var deserializer = new Deserializer();
            var Contacts = deserializer.Deserialize<Root>(input);
            return Contacts;*/
        }
        /*
        public static Root Retrieve(String filePath)
        {
            StreamReader input = File.OpenText(filePath);
            Deserializer deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            return deserializer.Deserialize<Root>(input);
        }
        */

        public static void WriteYaml(string Path, Root ConfigData)
        {
            File.WriteAllText("Content\\Mods\\VirtualBackpack\\test.yaml", "---\r\n");
            Serializer serializer = new Serializer();
            string WriteThis = serializer.Serialize(ConfigData);
            File.AppendAllText("Content\\Mods\\VirtualBackpack\\test.yaml", WriteThis);

        }
    }
}
