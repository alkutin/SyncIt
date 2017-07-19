using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SyncItAgent
{
    [Serializable]
    public class SyncConfiguration
    {
        [XmlArrayItem("Project")]
        public ProjectConfiguration[] Projects { get; set; }
    }
}
