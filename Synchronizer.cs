using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;


namespace FolderSync
{
    internal class Synchronizer
    {
        private readonly string source;
        private readonly string replica;
        private readonly Logger logger;
        
        public Synchronizer(string source, string replica, Logger logger)
        {
            this.source = source;  
            this.replica = replica; 
            this.logger = logger;
        }

        public void Synchronize()
        {
            logger.Log("Synchronization started");

            SynchronizeDirectories();
            SynchronizeFiles();
            CleanupReplica();

            logger.Log("Synchronization completed");
        }
        private void SynchronizeDirectories()
        {
            foreach (var dir in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(source, dir);
                var targetDir = Path.Combine(replica, relative);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                    logger.Log($"CREATE DIR: {targetDir}");
                }
            }
        }

        private void SynchronizeFiles()
        {
            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(source, file);
                var targetFile = Path.Combine(replica, relative);

                Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                if (!File.Exists(targetFile) || FilesDifferent(file, targetFile))
                {
                    CopyFile(file, targetFile);
                    logger.Log(File.Exists(targetFile) ? $"UPDATE: '{relative}'" : $"COPY: '{relative}'");
                }
            }
        }

        private void CopyFile(string file, string targetFile)
        {
            var tempFile = targetFile + ".tmp" + Guid.NewGuid().ToString("N");
            try
            {
                File.Copy(file, tempFile, true);
                File.Delete(targetFile);
                File.Move(tempFile, targetFile);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Copy failed: {e.Message}");
                // Ensure temp file is deleted if something went wrong
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch (Exception e2) { Console.Error.WriteLine($"Error: {e2.Message}"); }
                }
            }
        }

        private void CleanupReplica()
        {
            // Delete files not in source
            foreach (var file in Directory.EnumerateFiles(replica, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(replica, file);
                var sourceFile = Path.Combine(source, relative);
                if (!File.Exists(sourceFile))
                {
                    File.Delete(file);
                    logger.Log($"DELETE FILE: '{relative}'");
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(replica, "*", SearchOption.AllDirectories)
                                                .OrderByDescending(d => d.Length))
            {
                var relative = Path.GetRelativePath(replica, dir);
                var sourceDir = Path.Combine(source, relative);
                if (!Directory.Exists(sourceDir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        logger.Log($"DELETE DIR: '{relative}'");
                    }
                    catch (Exception e)
                    {
                        logger.Log($"Failed to delete dir '{dir}': {e.Message}");
                    }
                }
            }
        }

        private static bool FilesDifferent(string filePathA, string filePathB)
        {
            try
            {
                var infoA = new FileInfo(filePathA);
                var infoB = new FileInfo(filePathB);

                //file size different
                if (infoA.Length != infoB.Length) return true;

                return !HashesEqual(filePathA, filePathB);
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static bool HashesEqual(string filePathA, string filePathB)
        {
            try
            {
                using var md5 = MD5.Create();

                using var fileAStream = File.OpenRead(filePathA);
                byte[] hashA = md5.ComputeHash(fileAStream);

                using var fileBStream = File.OpenRead(filePathB);
                byte[] hashB = md5.ComputeHash(fileBStream);

                return hashA.SequenceEqual(hashB);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
