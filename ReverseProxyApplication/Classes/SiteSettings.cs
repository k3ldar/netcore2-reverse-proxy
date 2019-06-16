using System.Net.Http;

namespace ReverseProxyApplication
{
    public class SiteSettings
    {
        internal readonly HttpClient httpClient = new HttpClient();

        public string Name { get; set; }

        public string[] Bindings { get; set; }

        public string CertificateName { get; set; }

        public string CertificatePassword { get; set; }

        public string[] Endpoints { get; set; } 

    }
}
