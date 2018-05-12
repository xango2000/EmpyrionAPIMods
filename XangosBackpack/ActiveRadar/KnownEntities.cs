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
            var input = File.OpenText(filePath);
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

        }
    }
}
