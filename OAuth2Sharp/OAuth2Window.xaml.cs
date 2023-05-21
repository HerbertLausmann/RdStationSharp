using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Web;
using CefSharp;


namespace OAuth2Sharp
{
    /// <summary>
    /// Interaction logic for OAuth2Window.xaml
    /// </summary>
    partial class OAuth2Window : Window
    {
        public OAuth2Window()
        {
            CefSettings settings = new CefSettings();
            // path to directory 
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromecache");

            // Set the path 
            settings.CachePath = path;

            // set the options 
            Cef.Initialize(settings);
            InitializeComponent();
            //create settings variable 
        }

        private void ChromiumWebBrowser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (cefBrowser.Address.StartsWith(_CallBackUrl))
            //{
            //    _Token = cefBrowser.Address.Split('=')[1];
            //    this.Close();
            //}
            if (cefBrowser.Address.StartsWith(_CallBackUrl))
            {
                var queryParams = HttpUtility.ParseQueryString(cefBrowser.Address.Split('?')[1]);
                // Acesse os valores dos parâmetros por suas chaves.
                if (queryParams != null)
                {
                    if (queryParams.AllKeys.Contains("code"))
                    {
                        _Result = new OAuthResult(queryParams["code"], OAuthResultType.Success);
                    }
                    else
                    {
                        _Result = new OAuthResult(null, OAuthResultType.Failed);
                    }
                }
                else
                    _Result = new OAuthResult(null, OAuthResultType.Failed);
                this.Close();
            }
        }

        private string _OAuthUrl;
        private OAuthResult _Result;
        private string _CallBackUrl;

        public OAuthResult ShowDialog(string AuthURL, string CallBackURL)
        {
            _OAuthUrl = AuthURL;
            _CallBackUrl = CallBackURL;
            this.ShowDialog();
            return _Result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cefBrowser.Load(_OAuthUrl);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CefSharp.Cef.PreShutdown();
            CefSharp.Cef.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_Result is null)
            {
                _Result = new OAuthResult(null, OAuthResultType.Cancelled);
            }
        }
    }
}
