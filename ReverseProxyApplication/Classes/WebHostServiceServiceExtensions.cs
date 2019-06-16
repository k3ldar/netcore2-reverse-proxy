using Microsoft.AspNetCore.Hosting;

namespace ReverseProxyApplication
{
    public static class WebHostServiceServiceExtensions
    {
        public static void RunReverseProxyAsService(this IWebHost host)
        {
            var webHostService = new ServiceHost(host);
            ServiceHost.Run(webHostService);
        }
    }
}
