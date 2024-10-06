using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace CsharpFileSynchronizer
{
    public class FileSynchronizer
    {
        private readonly ILogger<Logger> _logger;
        private string _source;
        private string _backup;
        private FileHelper _fileHelper;
       

        public FileSynchronizer(ILogger<Logger> logger, string source, string backup) 
        {
            _source = source;
            _backup = backup;
            _logger = logger;
            _fileHelper = new FileHelper(logger);
        }

        private string CalculateMD5(string filePath)
        {
            // Open the file as a FileStream
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    // Compute the MD5 hash
                    byte[] hashBytes = md5.ComputeHash(stream);

                    // Convert the byte array to a hex string
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public void SyncDirectories()
        {
            var synchronizationNeeded = false;
            List<string> sourceFolder = _fileHelper.GetAllDirectories(_source);
            List<string> backupFolders = _fileHelper.GetAllDirectories(_backup);

            // Determine files to copy, delete, or update
            var folderToCopy = sourceFolder.Except(backupFolders); // Folders present in source but not in backup
            var foldersToDelete = backupFolders.Except(sourceFolder); // Folders present in backup but not in source

            if (folderToCopy.Count() > 0 || foldersToDelete.Count() > 0)
            {
                synchronizationNeeded = true;
            }

            // Copy new files from source to backup
            foreach (var folder in folderToCopy)
            {
                _fileHelper.CreateDirectory(_backup, folder);
            }

            // Delete files from backup that don't exist in source
            foreach (var folder in foldersToDelete)
            {
                _fileHelper.RemoveDirectory(_backup,folder);
            }
            if (synchronizationNeeded)
            {
                _logger.LogInformation("Synchronization complete.");
            }
        }

            // Synchronization of all files
        public void SyncFiles()
        {
            var synchronizationNeeded = false;
            // Get all files from source and backup directories
            //HashSet<string> sourceFiles = GetAllFiles(_source);
            //HashSet<string> backupFiles = GetAllFiles(_backup);
            List<string> sourceFiles = _fileHelper.GetAllFiles(_source);
            List<string> backupFiles = _fileHelper.GetAllFiles(_backup);

            // Determine files to copy, delete, or update
            var filesToCopy = sourceFiles.Except(backupFiles); // Files present in source but not in backup
            var filesToDelete = backupFiles.Except(sourceFiles); // Files present in backup but not in source
            var filesToUpdate = sourceFiles.Intersect(backupFiles).Where(file => NeedsUpdate(file)); // Files present in both but need update

            if (filesToCopy.Count() > 0 || filesToDelete.Count() > 0 || filesToUpdate.Count() > 0)
            {
                synchronizationNeeded = true;
            }

                // Copy new files from source to backup
            foreach (var file in filesToCopy)
            {
                _fileHelper.CopyFile(_backup, _source, file);
            }

            // Update modified files in backup
            foreach (var file in filesToUpdate)
            {
                _fileHelper.CopyFile(_backup, _source, file);
            }

            // Delete files from backup that don't exist in source
            foreach (var file in filesToDelete)
            {
                _fileHelper.RemoveFile(_backup, file);
            }

            if (synchronizationNeeded)
            {
                _logger.LogInformation("Synchronization complete.");
            }
        }

        // Method to check if a file needs to be updated
        private bool NeedsUpdate(string relativePath)
        {
            string sourceFile = Path.Combine(_source, relativePath);
            string backupFile = Path.Combine(_backup, relativePath);

            // Check if both files exist before comparing
            if (File.Exists(sourceFile) && File.Exists(backupFile))
            {
                // Compare MD5 hashes
                return CalculateMD5(sourceFile) != CalculateMD5(backupFile);
            }

            // If the backup file doesn't exist, it needs to be updated (copied)
            return true;
        }
        

        public void SyncFolders()
        {
            SyncDirectories();
            SyncFiles();
        }

        // Method to synchronize folders
        public void StartSync(int interval)
        {
            _logger.LogInformation("Starting folder synchronization...");
            try
            {
                while (true)
                {
                    SyncFolders();  // Replace with your actual synchronization Console.WriteLineic
                    Thread.Sleep(interval * 1000); // Sleep for the specified interval (converted to milliseconds)
                }
            }
            catch (ThreadInterruptedException)
            {
                _logger.LogError("Folder synchronization terminated.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during synchronization: {ex.Message}.");
            }
        }
    }
}
