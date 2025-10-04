using RD.Core.Interfaces;
using RD.Core.Models;
using System.Net;
using System.Text;

namespace RD.Core.Services;

internal class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(DownloadOptions options)
    {
        var handler = new HttpClientHandler();

        // Configure proxy
        if (options.Proxy != null)
        {
            var proxy = new WebProxy($"http://{options.Proxy.Host}:{options.Proxy.Port}");
            
            if (!string.IsNullOrEmpty(options.Proxy.Username))
            {
                proxy.Credentials = new NetworkCredential(options.Proxy.Username, options.Proxy.Password);
            }
            else if (options.Proxy.UseDefaultCredentials)
            {
                proxy.UseDefaultCredentials = true;
            }

            handler.Proxy = proxy;
        }

        var client = new HttpClient(handler)
        {
            Timeout = options.Timeout
        };

        // Configure authentication
        if (options.Authentication != null)
        {
            ConfigureAuthentication(client, options.Authentication);
        }

        // Set common headers
        client.DefaultRequestHeaders.Add("User-Agent", 
            "RD-DownloadManager/1.0 (Advanced HTTP Download Manager)");

        return client;
    }

    private static void ConfigureAuthentication(HttpClient client, AuthenticationConfiguration auth)
    {
        switch (auth.Type)
        {
            case AuthenticationType.Basic:
                if (!string.IsNullOrEmpty(auth.Username) && !string.IsNullOrEmpty(auth.Password))
                {
                    var credentials = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{auth.Username}:{auth.Password}"));
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case AuthenticationType.Bearer:
                if (!string.IsNullOrEmpty(auth.Token))
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
                }
                break;

            case AuthenticationType.ApiKey:
                if (!string.IsNullOrEmpty(auth.ApiKey) && !string.IsNullOrEmpty(auth.HeaderName))
                {
                    client.DefaultRequestHeaders.Add(auth.HeaderName, auth.ApiKey);
                }
                break;

            case AuthenticationType.Custom:
                if (!string.IsNullOrEmpty(auth.Token) && !string.IsNullOrEmpty(auth.HeaderName))
                {
                    client.DefaultRequestHeaders.Add(auth.HeaderName, auth.Token);
                }
                break;
        }
    }
}