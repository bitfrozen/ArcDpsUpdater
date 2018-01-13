using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcDpsUpdater
{
    internal class Program
    {
        private const string hashPath = @"C:\Program Files\Guild Wars 2\bin64\d3d9.dll.md5sum";
        private const string dllPath = @"C:\Program Files\Guild Wars 2\bin64\d3d9.dll";
        private const string dllUri = "https://www.deltaconnected.com/arcdps/x64/d3d9.dll";
        private const string hashUri = "https://www.deltaconnected.com/arcdps/x64/d3d9.dll.md5sum";
        private const string gameExe = @"C:\Program Files\Guild Wars 2\Gw2-64.exe";
        private static readonly HttpClient httpClient = new HttpClient();

        private static void Main()
        {
            RunAsync().Wait();
            Process.Start(gameExe);
        }

        private static async Task RunAsync()
        {
            Console.WriteLine("checking for ArcDps updates");

            var serverHash = await GetServerHashAsync();
            var localHash = GetLocalHash();

            if (serverHash != localHash)
            {
                Console.WriteLine("updating ArcDps");
                await UpdateAsync(serverHash);
                Console.WriteLine("ArcDps update complete");
            }
        }

        private static string GetLocalHash()
        {
            return File.Exists(hashPath) ? File.ReadAllText(hashPath) : string.Empty;
        }

        private static async Task UpdateAsync(string serverHash)
        {
            var response = await httpClient.GetAsync(dllUri);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(dllPath, bytes);
            File.WriteAllText(hashPath, serverHash);
        }

        private static async Task<string> GetServerHashAsync()
        {
            var response = await httpClient.GetAsync(hashUri);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
