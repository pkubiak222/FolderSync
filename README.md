# FolderSync
FolderSync is a simple tool to periodically synchronize the contents of a source folder to a replica folder.

## Usage
Run the program from the command line with 4 arguments:
FolderSync.exe <SourcePath> <ReplicaPath> <IntervalInSeconds> <LogFile>

### Parameters
- *SourcePath*: Path to the folder you want to copy from.
- *ReplicaPath*: Path to the folder you want to synchronize to.
- *IntervalInSeconds*: How often to synchronize (in seconds).
- *LogFile*: Path to the log file to save synchronization logs.

### Example
FolderSync.exe "C:\MySource" "D:\MyReplica" 10 "C:\Logs\sync.log"


### Notes
- Press *ESC* in the console to stop the synchronization.
- Source and replica paths must not be the same.
- Make sure the program has write permissions for the replica folder and log file.
