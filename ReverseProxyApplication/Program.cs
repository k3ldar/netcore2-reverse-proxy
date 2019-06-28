using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ReverseProxyApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;

            
            string exePath = Path.GetTempPath();

            Shared.EventLog.Initialise(7, Path.Combine(exePath, "Logs"), Path.Combine(exePath, "Errors"));

            if (isService)
            {
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            var builder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray());

            var host = builder.Build();

            if (isService)
            {
                // To run the app without the CustomWebHostService change the
                // next line to host.RunAsService();
                host.RunReverseProxyAsService();
            }
            else
            {
                host.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureKestrel((context, options) =>
                {
                    //options.ListenAnyIP(80, listenOptions =>
                    //{

                    //});
                    options.ListenAnyIP(81, listenOptions =>
                    {

                    });
                    //options.ListenAnyIP(443, listenOptions =>
                    //{
                    //    listenOptions.UseHttps(httpsOptions =>
                    //    {
                    //        CertificateSelector certificateSelector = new CertificateSelector();

                    //        httpsOptions.ServerCertificateSelector = (connectionContext, name) =>
                    //        {
                    //            return certificateSelector.SelectCertificate(connectionContext, name);
                    //        };
                    //    });
                    //});
                });
    }
}
