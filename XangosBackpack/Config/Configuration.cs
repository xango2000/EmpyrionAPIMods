using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;


namespace XangosAPIToolModule
{
    public class Configuration
    {
        public class Command
        {
            public class Permitted
            {
                public Int32 Admin { get; set; }
                public List<string> Player { get; set; }
                public List<string> Faction { get; set; }
            }
            public class NotPermitted
            {
                public List<string> Playfield { get; set; }
                public List<string> Faction { get; set; }
                public List<string> Player { get; set; }
            }
            public string UsageCost { get; set; }

        }
        public List<Command> Commands { get; set;}
    }
}
