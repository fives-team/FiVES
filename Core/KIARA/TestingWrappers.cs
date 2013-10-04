using System;
using System.Net;

namespace KIARA
{
    public interface IWebClient
    {
        string DownloadString(string address);
    };

    public class WebClientWrapper : WebClient, IWebClient { };
}

