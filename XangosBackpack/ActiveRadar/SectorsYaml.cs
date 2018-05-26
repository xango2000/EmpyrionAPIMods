using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;


namespace ActiveRadar
{
    class SectorsYaml
    {
        public static Contact Retrieve(String filePath)
        {
            var input = System.IO.File.OpenText(filePath);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            var Contacts = deserializer.Deserialize<Contact>(input);
            return Contacts;
        }
        public class Contact
        {
            public List<Sector> Sectors { get; set; }
        }
        public class Sector
        {
            public string Coordinates { get; set; }
            public string Color { get; set; }
            public string Icon { get; set; }
            public bool OrbitLine { get; set; }
            public string SectorMapType { get; set; }
            public string Allow { get; set; }
            public string Deny { get; set; }
            public List<string> Playfields { get; set; }
            
        }
    }
}
