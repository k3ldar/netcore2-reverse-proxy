using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Connections;

namespace ReverseProxyApplication
{
    internal sealed class CertificateSelector
    {
        #region Private Members

        private Settings _settings;

        #endregion Private Members

        #region Constructors

        internal CertificateSelector()
        {
            LoadSettings();
        }

        #endregion Constructors

        #region Internal Methods

        internal X509Certificate2 SelectCertificate(ConnectionContext connectionContext, string name)
        {
            SiteSettings siteSettings = _settings.Sites.Where(s => s.Name == name).FirstOrDefault();

            if (siteSettings != null)
            {
                string certFile = Path.Combine(_settings.Certificates, siteSettings.CertificateName);

                if (File.Exists(certFile))
                {
                    //return CertificateLoader.LoadFromStoreCert(siteSettings.CertificateName, "My", StoreLocation.CurrentUser, true);
                    var c = new X509Certificate(certFile, siteSettings.CertificatePassword);
                    return new X509Certificate2(c.GetRawCertData(), siteSettings.CertificatePassword);
                }
            }

            return null;
        }

        #endregion Internal Methods

        #region Private Methods

        private void LoadSettings()
        {
            DefaultSettingProvider settingProvider = new DefaultSettingProvider();
            _settings = settingProvider.GetSettings<Settings>("Configuration");
        }

        #endregion Private Methods
    }
}
