using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Xunit.Abstractions;

namespace JumpGate
{
    public class JumpGateSettings
    {
        private readonly ITestOutputHelper output;
        
        public JumpGateSettings(ITestOutputHelper output)
        {
            this.output = output;
        }

        public static JumpGates Settings()
        {
            StringReader input = new StringReader("settings.yaml");

            Deserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            JumpGates jumpdata = deserializer.Deserialize<JumpGates>(input);
            string test = jumpdata.Playfield;
            return jumpdata;
        }
        
        public class JumpGates
        {
            public string Playfield { get; set; }
            public Int32 JumpGatePOI { get; set; }
            public Dictionary<string, Int32> Departure { get; set; }
            public string ToPlayfield { get; set; }
            public List<Destination> Destination { get; set; }
        }

        public class coords
        {
            public Int32 x { get; set; }
            public Int32 y { get; set; }
            public Int32 z { get; set; }
            public Int32 Radius { get; set; }
        }

        public class justcoords
        {
            public Int32 x { get; set; }
            public Int32 y { get; set; }
            public Int32 z { get; set; }
        }

        public class Destination
        {
            public Dictionary<string,Int32> Position { get; set; }
            public Dictionary<string, Int32> Rotation { get; set; }
        }
    }
}
