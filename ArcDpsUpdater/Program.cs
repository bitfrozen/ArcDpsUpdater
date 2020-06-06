using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VirusTotalNet;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;

namespace ArcDpsUpdater
{
    internal static class Program
    {
        private static string HashPath => ConfigurationManager.AppSettings["hashPath"];
        private static string DllPath => ConfigurationManager.AppSettings["dllPath"];
        private static string DllUri => ConfigurationManager.AppSettings["dllUri"];
        private static string HashUri => ConfigurationManager.AppSettings["hashUri"];
        private static string GameExe => ConfigurationManager.AppSettings["gameExe"];
        private static string VirusTotalApiKey => ConfigurationManager.AppSettings["virusTotalApiKey"];
        private static readonly HttpClient HttpClient = new HttpClient();

        private static async Task Main()
        {
            // UpdateAsync().Wait();
            if (await UpdateAsync())
            {
                Process.Start(GameExe);                
            }
        }

        private static async Task<bool> UpdateAsync()
        {
            Console.WriteLine("checking for ArcDps updates");

            var serverHash = await GetServerHashAsync();
            var localHash = GetLocalHash();

            if (serverHash != localHash)
            {
                Console.WriteLine("updating ArcDps");
                await UpdateAsync(serverHash);

                if (string.IsNullOrWhiteSpace(VirusTotalApiKey))
                {
                    await Console.Out.WriteLineAsync("Skipping VirusTotal check, because no API key specified in config file");
                }
                else
                {
                    if (!await CheckFileWithVirusTotal()) return false;
                }

                Console.WriteLine("ArcDps update complete");
            } else
            {
                Console.WriteLine("nothing to update");
            }

            return true;
        }

        private static async Task<bool> CheckFileWithVirusTotal()
        {
            await Console.Out.WriteLineAsync("Checking ArcDps update on VirusTotal");
            var fileReport = await GetVirusTotalFileReport(DllPath);
            if (fileReport.Positives > 0)
            {
                await Console.Out.WriteLineAsync("Some antivirus detected this update as positive. Here is the report:");
                foreach (var scan in fileReport.Scans.Where(scan => scan.Value.Detected))
                {
                    await Console.Out.WriteLineAsync($"{scan.Key} detected as positive");
                }

                await Console.Out.WriteAsync("Do you want to continue? Y/N default: No");
                var userResponse = await Console.In.ReadLineAsync();
                if (!userResponse.ToLower().Contains("y"))
                {
                    return false;
                }
            }
            await Console.Out.WriteLineAsync("Finished ArcDps check");

            return true;
        }

        private static async Task<FileReport> GetVirusTotalFileReport(string dllPath)
        {
            await Console.Out.WriteLineAsync($"Checking file: {dllPath} on VirusTotal");
            var virusTotal =
                new VirusTotal(VirusTotalApiKey) {UseTLS = true};

            //Check if the file has been scanned before.
            var fileToScan = new FileInfo(dllPath);
            var fileReport = await virusTotal.GetFileReportAsync(fileToScan);
            var hasFileBeenScannedBefore = fileReport.ResponseCode == FileReportResponseCode.Present;
            //If the file has been scanned before, the results are embedded inside the report.
            if (!hasFileBeenScannedBefore)
            {
                ScanResult fileResult = await virusTotal.ScanFileAsync(fileToScan);
                fileReport = await virusTotal.GetFileReportAsync(fileToScan);
            }

            return fileReport;
        }

        private static string GetLocalHash()
        {
            return File.Exists(HashPath) ? File.ReadAllText(HashPath) : string.Empty;
        }

        private static async Task UpdateAsync(string serverHash)
        {
            var response = await HttpClient.GetAsync(DllUri);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(DllPath, bytes);
            File.WriteAllText(HashPath, serverHash);
        }

        private static async Task<string> GetServerHashAsync()
        {
            var response = await HttpClient.GetAsync(HashUri);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
