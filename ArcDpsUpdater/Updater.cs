using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace ArcDpsUpdater
{
    public class Updater
    {
        private const string hashPath = @"C:\Program Files\Guild Wars 2\bin64\d3d9.dll.md5sum";
        private const string dllPath = @"C:\Program Files\Guild Wars 2\bin64\d3d9.dll";
        private const string dllUri = "https://www.deltaconnected.com/arcdps/x64/d3d9.dll";
        private const string hashUri = "https://www.deltaconnected.com/arcdps/x64/d3d9.dll.md5sum";
        private static readonly HttpClient httpClient = new HttpClient();

        private readonly Timer _timer;

        public Updater()
        {
#if DEBUG
            _timer = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds) { AutoReset = true };
#else
            _timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds) { AutoReset = true };
#endif
            _timer.Elapsed += (sender, eventArgs) => RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            Console.WriteLine("checking for updates");

            var serverHash = await GetServerHashAsync();
            var localHash = GetLocalHash();

            if (serverHash != localHash)
            {
                Console.WriteLine("updating");
                await UpdateAsync(serverHash);
                Console.WriteLine("update complete");
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

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}