using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ActiveRadar
{
    class KnownEntities
    {
        public static Contact Retrieve(String filePath)
        {
            var input = System.IO.File.OpenText(filePath);
            var deserializer = new Deserializer();
            var Contacts = deserializer.Deserialize<Contact>(input);
            return Contacts;
        }
        public class Contact
        {
            public List<Ident> Scanned { get; set; }
        }
        public class Ident
        {
            public int ID { get; set; }
            public int Power { get; set; }
            public List<ItemStacks> Components { get; set; }

        }
        public class ItemStacks
        {
            public int SlotIdx { get; set; }
            public int ID { get; set; }
            public int Count { get; set; }
            public int Ammo { get; set; }
            public int Decay { get; set; }
        }

        public static void WriteYaml(int EntityId, Contact ContactData)
        {
            System.IO.File.WriteAllText("Content\\Mods\\ActiveRadar\\test.yaml", "---\r\n");
            Serializer serializer = new Serializer();
            string WriteThis = serializer.Serialize(ContactData);
            System.IO.File.AppendAllText("Content\\Mods\\ActiveRadar\\test.yaml", WriteThis);
        }
    }
}