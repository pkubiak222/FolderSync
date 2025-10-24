using FolderSync;
using System.IO;
using System.Timers;

internal class Program
{

    private static int Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Not enough arguments. Please provide arguments: SourcePath ReplicaPath IntervalInSeconds LogFile");
            return 1;
        }

        string sourcePath = args[0];
        string replicaPath = args[1];

        //TryParse will return false if failed to parse string to int
        if (!int.TryParse(args[2], out int intervalInSeconds) || intervalInSeconds < 0)
        {
            Console.WriteLine("Interval is not a positive number. Please provide interval in seconds");
            return 1;
        }

        if (intervalInSeconds == 0)
        {
            Console.WriteLine("Interval cannot be 0");
            return 1;
        }

        if (replicaPath == sourcePath)
        {
            Console.WriteLine("Source and Replica paths cannot be the same");
            return 1;
        }
        
        string logFile = args[3];

        //if(logFile.Length == 0)
        if(!Directory.Exists(sourcePath))
        {
            Console.WriteLine("Source folder does not exist. Please provide valid source path");
            return 1;
        }

        try
        {
            string fullReplicaPath = Path.GetFullPath(replicaPath); 

            if (!Directory.Exists(fullReplicaPath))
            {
                Directory.CreateDirectory(fullReplicaPath);
                Console.WriteLine($"Replica directory created at: {fullReplicaPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid replica path '{replicaPath}': {ex.Message}");
            return 1;
        }

        var logFilePath = Path.GetFullPath(logFile);
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath) ?? ".");

        Logger logger = new Logger(logFile);

        Synchronizer synchronizer = new Synchronizer(sourcePath, replicaPath, logger);

        Console.WriteLine("Press ESC to stop synchronization.");
        logger.Log($"Starting periodic synchronization every {intervalInSeconds} seconds...");

        bool stopRequested = false;

        while (!stopRequested)
        {
            try
            {
                synchronizer.Synchronize();
                logger.Log($"Synchronization completed at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                logger.Log($"Error during synchronization: {ex.Message}");
            }

            // Wait for the interval in small chunks to detect ESC key
            int waited = 0;
            while (waited < intervalInSeconds * 1000 && !stopRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        stopRequested = true;
                        logger.Log("Stopping synchronization...");
                        break;
                    }
                }

                Thread.Sleep(100); // small delay to avoid busy waiting
                waited += 100;
            }
        }

        logger.Dispose();
        Console.WriteLine("Synchronization stopped.");
        return 0;
    }
}

