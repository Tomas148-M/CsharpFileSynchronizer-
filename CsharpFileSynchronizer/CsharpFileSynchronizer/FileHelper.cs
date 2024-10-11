using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
                //// Attempt to copy the file from source to backup
                //File.Copy(sourceFile, backupFile, true); // Overwrite if it exists

                //_logger.LogInformation($"File {sourceFile} was successfully copied to {backupFile}.");
                // Pokusíme se kopírovat soubor, dokud nebude stabilní
                const int maxRetries = 100;
                const int waitIntervalMs = 100;

                int attempts = 0;

                if (IsFileReady(sourceFile))
                {
                    File.Copy(sourceFile, backupFile, true); // Přepíšeme, pokud již existuje
                }
                else
                {
                    _logger.LogInformation($"File changing");

                    while (attempts < maxRetries)
                    {
                        if (IsFileStable(sourceFile))
                        {
                            // Soubor je stabilní, můžeme jej zkopírovat
                            File.Copy(sourceFile, backupFile, true); // Přepíšeme, pokud již existuje

                            _logger.LogInformation($"File {sourceFile} was successfully copied to {backupFile}.");
                            break; // Ukončíme smyčku, protože kopírování proběhlo úspěšně
                        }
                        else
                        {
                            _logger.LogWarning($"File {sourceFile} is not stable, retrying in {waitIntervalMs / 1000} seconds...");
                            Thread.Sleep(waitIntervalMs); // Počkáme určený interval
                            attempts++;
                        }
                    }

                    // Pokud po maximálním počtu pokusů nebyl soubor stabilní
                    if (attempts == maxRetries)
                    {
                        _logger.LogError($"File {sourceFile} could not be copied after {maxRetries} attempts.");
                    }
                }
            }
            catch (IOException ex)
            {
               
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

        public bool IsFileReady(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true; // Pokud se stream otevře, soubor je připravený ke kopírování.
                }
            }
            catch (IOException)
            {
                return false; // Soubor je stále používán a není připraven ke kopírování.
            }
        }

        // Kontrola, zda je soubor stabilní (jeho velikost se nezmění během intervalu)
        private bool IsFileStable(string filePath, int checkIntervalMs = 100)
        {
            try
            {
                // Získáme počáteční velikost souboru
                long initialSize = new FileInfo(filePath).Length;

                // Počkáme určený interval (např. 1 sekundu)
                Thread.Sleep(checkIntervalMs);

                // Znovu zkontrolujeme velikost souboru
                long newSize = new FileInfo(filePath).Length;

                // Pokud se velikost nezměnila, soubor je stabilní
                return initialSize == newSize;
            }
            catch (IOException ex)
            {
                // Pokud došlo k chybě při přístupu k souboru, vrátíme false (soubor není stabilní)
                _logger.LogError(ex, $"Error accessing file {filePath}.");
                return false;
            }
        }
    }
}
