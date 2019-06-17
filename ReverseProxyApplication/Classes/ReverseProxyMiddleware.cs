using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace ReverseProxyApplication
{
    public class ReverseProxyMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;

        public ReverseProxyMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext context)
        {
            SiteSettings siteSettings = null;

            var targetUri = BuildTargetUri(context.Request, ref siteSettings);
            try
            {
                if (targetUri != null && siteSettings != null)
                {
                    var targetRequestMessage = CreateTargetMessage(context, targetUri);

                    using (var responseMessage = await siteSettings.httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                    {
                        context.Response.StatusCode = (int)responseMessage.StatusCode;

                        CopyFromTargetResponseHeaders(context, responseMessage);

                        await ProcessResponseContent(context, responseMessage);
                    }

                    return;
                }
            }
            catch (Exception err)
            {
                string s = err.Message;
                throw;
            }

            await _nextMiddleware(context);
        }

        private async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsByteArrayAsync();
            await context.Response.Body.WriteAsync(content);
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = new HttpMethod(context.Request.Method);
           
            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            context.Response.Headers.Remove("transfer-encoding");
        }

        private Uri BuildTargetUri(HttpRequest request, ref SiteSettings siteSettings)
        {
            DefaultSettingProvider settingProvider = new DefaultSettingProvider();
            var settings = settingProvider.GetSettings<Settings>("Configuration");

            siteSettings = settings.Sites.Where(s => s.Bindings.Contains(request.Host.Host)).FirstOrDefault();

            if (siteSettings != null)
            {
                // we could have a web farm here, serve the request to multiple endpoints?
                return new Uri(siteSettings.Endpoints[0] + request.Path);
            }

            return new Uri("http://localhost:63812" + request.Path);
        }
    }
}
