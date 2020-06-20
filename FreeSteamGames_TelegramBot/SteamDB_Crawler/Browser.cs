using System;
using System.IO;
using System.Threading;
using CefSharp;
using CefSharp.OffScreen;

namespace SteamDB_Crawler
{
    class Browser
    {
        private static ChromiumWebBrowser browser;

        public delegate void OnSourceCodeLoaded(string src);
        public static OnSourceCodeLoaded OnSourceCodeLoadedEvent;

        public static void OpenUrl(string url)
        {
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            var settings = new CefSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(
                                         Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            if (browser != null)
                browser.Load(url);
            else
            {
                browser = new ChromiumWebBrowser(url);
                browser.LoadingStateChanged += BrowserLoadingStateChanged;
            }
        }

        private static void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                e.Browser.MainFrame.GetSourceAsync().ContinueWith(taskHtml =>
                {
                    var html = taskHtml.Result;
                    OnSourceCodeLoadedEvent?.Invoke(html);
                });
            }
        }

    }
}