using System.IO;

internal class Program
{
    private static int Main(string[] args)
    {
        if(args.Length != 4)
        {
            Console.WriteLine("Not enough arguments. Please provide arguments: SourcePath ReplicaPath IntervalInSeconds LogFile");
            return 1;
        }

        string sourcePath = args[0];
        string replicaPath = args[1];
        
        //TryParse will return false if failed to parse string to int
        if (!int.TryParse(args[2], out int intervalInSeconds) || intervalInSeconds <= 0)
        {
            Console.WriteLine("Interval is not a positive number. Please provide interval in seconds");
            return 1;
        }
        
        string logFile = args[3];

        //if(logFile.Length == 0)
        if(!Directory.Exists(sourcePath))
        {
            Console.WriteLine("Source folder does not exist. Please provide valid source path");
            return 1;
        }

        //creating replica folder
        Directory.CreateDirectory(replicaPath);

        var logFilePath = Path.GetFullPath(logFile);
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath) ?? ".");

        //TODO logging

        return 0;
    }
}