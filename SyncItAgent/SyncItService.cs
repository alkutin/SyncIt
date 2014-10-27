using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace SyncItAgent
{    
    public class SyncItService
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        public SyncConfiguration Configuration { get; private set; }
        public SyncItService(SyncConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void SyncIt()
        {
            foreach (var project in Configuration.Projects)
            {
                SyncProject(project);
            }
        }

        protected void SyncProject(ProjectConfiguration project)
        {
            Console.WriteLine("Syncing {0}", project.Name);
            foreach (var folder in project.Folders)
            {
                SyncFolder(folder);
            }
        }

        protected void SyncFolder(FolderConfiguration folder)
        {
            Console.WriteLine("Doing {0} from {1} to {2}", folder.Method, folder.Source, folder.Destination);

            var sourceFolders = Directory.GetDirectories(folder.Source, "*.*", SearchOption.AllDirectories).Select(s => s.Substring(folder.Source.Length + 1)).ToList();
            var targetFolders = Directory.GetDirectories(folder.Destination, "*.*", SearchOption.AllDirectories).Select(s => s.Substring(folder.Destination.Length + 1)).ToList();

            var sourceFiles = Directory.GetFiles(folder.Source, "*.*", SearchOption.AllDirectories).Select(s => s.Substring(folder.Source.Length + 1)).ToList();
            var targetFiles = Directory.GetFiles(folder.Destination, "*.*", SearchOption.AllDirectories).Select(s => s.Substring(folder.Destination.Length + 1)).ToList();

            var specialCareTargetItems = folder.SpecialCareItems != null ? folder.SpecialCareItems.Select(s => s.Destination).ToArray() : new string[0];

            var foldersToCreate = sourceFolders.Except(specialCareTargetItems).Except(targetFolders).OrderBy(o => o);
            foreach (var folderToCreate in foldersToCreate)
            {
                var path = Path.Combine(folder.Destination, folderToCreate);
                Console.WriteLine("Creating folder {0}", path);
                Directory.CreateDirectory(path);
            }

            var foldersToDelete = targetFolders.Except(specialCareTargetItems).Except(sourceFolders).Where(w => !string.IsNullOrEmpty(w));
            foreach (var folderToDelete in foldersToDelete)
            {
                var path = Path.Combine(folder.Destination, folderToDelete);
                if (Directory.Exists(path))
                {
                    Console.WriteLine("Deleting folder {0}", path);
                    Directory.Delete(path, true);
                }
            }

            var filesToDelete = targetFiles.Except(specialCareTargetItems).Except(sourceFiles).Where(w => !string.IsNullOrEmpty(w));
            foreach (var fileToDelete in filesToDelete)
            {
                var path = Path.Combine(folder.Destination, fileToDelete);                
                if (File.Exists(path))
                {
                    Console.WriteLine("Deleting file {0}", path);
                    File.Delete(path);
                }
            }

            var filesToUpdate = sourceFiles.Except(specialCareTargetItems).Except(filesToDelete).OrderBy(o => o);
            foreach (var fileToUpdate in filesToUpdate)
            {
                var sourcePath = Path.Combine(folder.Source, fileToUpdate);
                var targetPath = Path.Combine(folder.Destination, fileToUpdate);
                SyncFile(folder, sourcePath, targetPath);
            }

            if (folder.SpecialCareItems != null)
                foreach (var item in folder.SpecialCareItems)
                {
                    SyncSpecialItem(folder, item);
                }
        }

        private void SyncFile(FolderConfiguration folder, string sourcePath, string targetPath)
        {
            var targetExists = File.Exists(targetPath);
            var needUpdate = !targetExists || File.GetLastWriteTimeUtc(targetPath) != File.GetLastWriteTimeUtc(sourcePath);

            if (needUpdate)
            {
                switch (folder.Method)
                {
                    case SyncMethodType.Copy:
                        Console.WriteLine("Copying {0} to {1}", sourcePath, targetPath);
                        File.Copy(sourcePath, targetPath, true);
                        break;
                    case SyncMethodType.Hardlink:
                        if (targetExists)
                        {
                            Console.WriteLine("Deleting file {0}", targetPath);
                            File.Delete(targetPath);
                        }
                        Console.WriteLine("Creating hardlink at {1} for {0}", sourcePath, targetPath);
                        CreateHardLink(targetPath, sourcePath, IntPtr.Zero);
                        File.SetLastWriteTimeUtc(targetPath, File.GetLastWriteTimeUtc(sourcePath));
                        break;
                    default:
                        Console.WriteLine("Skipping {0}", targetPath);
                        break;
                }
            }
        }

        private void SyncSpecialItem(FolderConfiguration parent, FolderConfiguration item)
        {
            var sourcePath = Path.Combine(parent.Source, item.Source);
            var targetPath = Path.Combine(parent.Destination, item.Destination);

            var isFolder = Directory.Exists(sourcePath);
            var isFile = File.Exists(sourcePath);

            if (isFile)
            {
                var targetExists = File.Exists(targetPath);
                SyncFile(parent, sourcePath, targetPath);
            }

            if (isFolder)
            {
                var folder = new FolderConfiguration()
                {
                    Source = sourcePath,
                    Destination = targetPath,
                    Method = item.Method,
                    SpecialCareItems = item.SpecialCareItems
                };
                SyncFolder(folder);
            }
        }
    }
}
