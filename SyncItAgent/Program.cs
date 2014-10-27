using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncItAgent
{
    class Program
    {
        static SyncConfiguration _configuration;
        static void ShowHelp()
        {
            Console.WriteLine("Usage: SyncIt {settingsFile}");
            Console.WriteLine("Settings file example:");
            Console.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            Console.WriteLine("<projects>");
            Console.WriteLine(" <project name=\"AscendisWeb\">");
            Console.WriteLine("  <folder \r\n" + 
                "     source=\"D:\\Projects\\2010Collection\\AEM\\RELEASES\\AEM_1.0.0\\WEBSERVER\\Web\" \r\n" +
                "     destination=\"C:\\inetpub\\sites\\AscendisDev\" method=\"hardlink\">");
            Console.WriteLine("    <file source=\"kutin.web.config\" destination=\"web.config\" method=\"copy\" />");
            Console.WriteLine("  </folder>");
            Console.WriteLine(" </project>");
            Console.WriteLine("</projects>");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var configurationPath = args[0];
            var configuration = ConfigurationLoader.LoadConfiguration(configurationPath);

            bool listeningForChanges = false;

            var watchers = new List<FileSystemWatcher>();
            var projectCount = configuration.Projects != null ? configuration.Projects.Length : 0;
            Console.WriteLine("Found projects: {0}", projectCount);
            if (projectCount > 0)
            {
                Console.WriteLine(string.Join(", ", configuration.Projects.Select(s => s.Name).ToArray()));
                foreach (var project in configuration.Projects)
                {
                    Console.WriteLine("{0}. Folders: {1}. Listening for changes: {2}", project.Name, project.Folders != null ? project.Folders.Length : 0, project.ListenChanges);
                    if (project.Folders != null)
                    {
                        listeningForChanges = listeningForChanges || project.ListenChanges;
                        foreach (var folder in project.Folders)
                        {
                            Console.WriteLine("{2}: {0} -> {1}. Special items: {3}", folder.Method, folder.Source, folder.Destination,
                                folder.SpecialCareItems != null ? folder.SpecialCareItems.Length : 0);

                            if (project.ListenChanges)
                            {
                                var watcher = new FileSystemWatcher(folder.Source)
                                {
                                  Filter = "*.*",
                                  IncludeSubdirectories  = true,
                                  NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                  EnableRaisingEvents = false
                                };
                                watcher.Created += watcher_Created;
                                watcher.Renamed += watcher_Renamed;
                                watcher.Changed += watcher_Changed;
                                watcher.Deleted += watcher_Deleted;                                

                                watchers.Add(watcher);

                                watcher.EnableRaisingEvents = true;
                            }
                        }
                    }
                }                
            }

            _configuration = configuration;
            InvokeLoop();
            
            if (listeningForChanges)
            {
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                foreach (var watcher in watchers)
                    watcher.Dispose();
                watchers.Clear();
            }
        }

        static void InvokeLoop()
        {
            try
            {
                lock(_configuration)
                {
                    new SyncItService(_configuration).SyncIt();
                }
            }
            catch(Exception error)
            {
                Console.WriteLine(error.ToString());
            }
        }

        static void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            
            InvokeLoop();
        }

        static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            InvokeLoop();
        }

        static void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            InvokeLoop();
        }

        static void watcher_Created(object sender, FileSystemEventArgs e)
        {
            InvokeLoop();
        }
    }
}
