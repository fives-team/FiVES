using System;
using System.Net;

namespace KIARA
{
    public interface IWebClient
    {
        string DownloadString(string address);
    };

    public class WebClientWrapper : IWebClient
    {
        public string DownloadString(string address)
        {
            return webClient.DownloadString(address);
        }

        private WebClient webClient = new WebClient();
    };
}

