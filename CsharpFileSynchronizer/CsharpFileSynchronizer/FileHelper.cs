using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpFileSynchronizer
{
    public class FileHelper
    {
        private readonly ILogger<Logger> _logger;


        public FileHelper(ILogger<Logger> logger)
        {
            _logger = logger;
        }

        // Method to remove a directory based on relative path
        public void RemoveDirectory(string backup, string relativePath)
        {
            try
            {
                // Combine the backup path and the relative path to get the full directory path
                string fullPath = Path.Combine(backup, relativePath);

                // Check if the directory exists
                if (Directory.Exists(fullPath))
                {
                    // Delete the directory and its contents
                    Directory.Delete(fullPath, true); // 'true' specifies recursive deletion
                    _logger.LogInformation($"Directory deleted: {relativePath}");
                }
                else
                {
                    _logger.LogInformation($"Directory does not exist: {relativePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to remove directory {relativePath}: {ex.Message}");
            }
        }

        // Method to create a directory based on relative path
        public void CreateDirectory(string backup, string relativePath)
        {
            try
            {
                // Combine the backup path and the relative path to get the full directory path
                string fullPath = Path.Combine(backup, relativePath);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    _logger.LogInformation($"Directory created: {relativePath}");
                }
                else
                {
                    _logger.LogInformation($"Directory already exists: {relativePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating directory {relativePath}: {ex.Message}");
            }
        }


        // Method to remove a file and _logger.LogInformation the action
        public void RemoveFile(string backup, string relativePath)
        {
            string backupFile = Path.Combine(backup, relativePath);

            try
            {
                if (File.Exists(backupFile))
                {
                    File.Delete(backupFile);
                    _logger.LogInformation($"File deleted: {relativePath}");
                }
                else
                {
                    _logger.LogInformation($"File not found: {backupFile}");
                }
            }
            catch (IOException ex)
            {
                _logger.LogError($"Error deleting file {backupFile}: {ex.Message}");
            }
        }

        // Copy file, ensuring it is stable according to its size
        public void CopyFile(string backup, string source, string relativePath)
        {
            string sourceFile = Path.Combine(source, relativePath);
            string backupFile = Path.Combine(backup, relativePath);

            try
            {
                // Attempt to copy the file from source to backup
                File.Copy(sourceFile, backupFile, true); // Overwrite if it exists
                _logger.LogInformation($"File {sourceFile} was successfully copied to {backupFile}.");
            }
            catch (IOException ex)
            {
                _logger.LogError($"Failure during the copying of {sourceFile}: {ex.Message}");
                _logger.LogError("Attempting to copy file again after size stability check...");

                //// Perform size stability check
                //if (IsFileStable(sourceFile))
                //{
                //    try
                //    {
                //        File.Copy(sourceFile, backupFile, true); // Retry copying the file after stability check
                //        _logger.LogInformation($"File {sourceFile} was successfully copied to {backupFile} after stability check.");
                //    }
                //    catch (IOException retryEx)
                //    {
                //        _logger.LogInformation($"Copy failed again: {retryEx.Message}");
                //    }
                //}
                //else
                //{
                //    _logger.LogInformation($"File {sourceFile} is still unstable, copying aborted.");
                //}
            }
        }

        public List<string> GetAllDirectories(string path, bool includeSubdirectories = true)
        {
            if (Directory.Exists(path))
            {
                // Get directories based on the 'includeSubdirectories' flag
                var allDirectories = Directory.GetDirectories(path, "*",
                    includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                List<string> relativePaths = allDirectories.Select(directory => GetRelativePath(path, directory)).ToList();
                return relativePaths;
            }
            else
            {
                // Return an empty array if the directory doesn't exist
                _logger.LogInformation("The specified path does not exist.");
                return new List<string> { };
            }
        }

        public List<string> GetAllFiles(string path)
        {
            if (Directory.Exists(path))
            {
                // Get files based on the 'includeSubdirectories' flag
                var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList();
                // Convert absolute paths to relative paths
                List<string> relativePaths = allFiles.Select(file => GetRelativePath(path, file)).ToList();
                return relativePaths;
            }
            else
            {
                // Return an empty array if the directory doesn't exist
                _logger.LogInformation("The specified path does not exist.");
                return new List<string> { };
            }
        }

        private string GetRelativePath(string basePath, string filePath)
        {
            // Ujistěte se, že základní cesta končí lomítkem
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }

            // Zkontroluje, zda cesta k souboru začíná základní cestou
            if (filePath.StartsWith(basePath))
            {
                // Odstraní základní část z cesty
                return filePath.Substring(basePath.Length);
            }

            // Pokud cesta není pod základní cestou, vrátí originální cestu
            return filePath;
        }
    }
}
