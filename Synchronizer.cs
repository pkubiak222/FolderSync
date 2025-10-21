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
