using System.Collections.Generic;

namespace ReverseProxyApplication
{
    public class Settings
    {
        public string Certificates { get; set; }

        public List<SiteSettings> Sites { get; set; }
    }
}
