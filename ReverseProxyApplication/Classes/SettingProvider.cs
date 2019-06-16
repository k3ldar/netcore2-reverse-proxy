using System;

using Microsoft.Extensions.Configuration;

namespace ReverseProxyApplication
{
    internal sealed class DefaultSettingProvider
    {
        #region Public Methods

        public T GetSettings<T>(in string storage, in string sectionName)
        {
            if (String.IsNullOrEmpty(storage))
                throw new ArgumentNullException(nameof(storage));

            if (String.IsNullOrEmpty(sectionName))
                throw new ArgumentNullException(nameof(sectionName));

            ConfigurationBuilder builder = new ConfigurationBuilder();
            IConfigurationBuilder configBuilder = builder.SetBasePath(GetPath());
            configBuilder.AddJsonFile(storage);
            IConfigurationRoot config = builder.Build();
            T Result = (T)Activator.CreateInstance(typeof(T));
            config.GetSection(sectionName).Bind(Result);

            return Result;
        }

        public T GetSettings<T>(in string sectionName)
        {
            return GetSettings<T>("appsettings.json", sectionName);
        }

        #endregion Public Methods

        #region Private Methods

        private string GetPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        #endregion Private Methods
    }
}
