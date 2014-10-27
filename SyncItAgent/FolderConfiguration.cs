using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SyncItAgent
{
    [Serializable]
    public class FolderConfiguration
    {
        [XmlAttribute]
        public string Source { get; set; }

        [XmlAttribute]
        public string Destination { get; set; }

        [XmlAttribute]
        public SyncMethodType Method { get; set; }

        [XmlArrayItem("Item")]
        public FolderConfiguration[] SpecialCareItems { get; set; }
    }
}
