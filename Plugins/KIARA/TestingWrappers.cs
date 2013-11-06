using System;
using System.Net;

namespace KIARAPlugin
{
    public interface IWebClient
    {
        string DownloadString(string address);
    };

    public class WebClientWrapper : WebClient, IWebClient { };
}

