using System;
using Topshelf;

namespace ArcDpsUpdater
{
    internal class Program
    {
        private static void Main()
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<Updater>(s =>
                {
                    s.ConstructUsing(name => new Updater());
                    s.WhenStarted(updater => updater.Start());
                    s.WhenStopped(updater => updater.Stop());
                });

                x.RunAsLocalSystem();
                x.SetDescription("Automatic updater for Guild Wars 2 ArcDps plugin");
                x.SetDisplayName("ArcDps Updater");
                x.SetServiceName("ArcDpsUpdater");
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
