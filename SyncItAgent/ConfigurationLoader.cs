﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SyncItAgent
{
    public class ConfigurationLoader
    {
        public static SyncConfiguration LoadConfiguration(string path)
        {
            Debug.WriteLine(typeof(SyncConfiguration).FullName);
            var serializer = new XmlSerializer(typeof(SyncConfiguration));
            using (var stream = File.OpenRead(path))
            {
                return (SyncConfiguration)serializer.Deserialize(stream);
            }
        }

    }
}
