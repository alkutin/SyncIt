using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SyncItAgent
{
    [Serializable]
    public class ProjectConfiguration
    {        
        [XmlAttribute]
        public string Name { get; set; }

        [XmlArrayItem("Folder")]
        public FolderConfiguration[] Folders { get; set; }

        [XmlAttribute]
        public bool ListenChanges { get; set; }
    }
}
