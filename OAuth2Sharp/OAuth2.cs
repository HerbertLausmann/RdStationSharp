using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OAuth2Sharp
{
    public class OAuth2
    {
        /// <summary>
        /// Authenticates the user asynchronously using the OAuth2 protocol and returns the authorization token.
        /// </summary>
        /// <param name="authUrl">The OAuth2 authorization URL to start the authentication process.</param>
        /// <param name="callbackUrl">The callback URL to which the OAuth2 server will redirect the user after the authentication process.</param>
        /// <returns>A task that represents the asynchronous authentication operation. The result of the task contains the authorization token as a string if the authentication is successful, otherwise null.</returns>
        public static async Task<OAuthResult> AuthAsync(string authUrl, string callbackUrl)
        {
            OAuthResult token = null;
            Thread th = new Thread(new ThreadStart(() =>
            {
                OAuth2Window oAuth2Window = new OAuth2Window();
                token = oAuth2Window.ShowDialog(authUrl, callbackUrl);
            }));
            th.SetApartmentState(ApartmentState.STA);
            th.Priority = ThreadPriority.AboveNormal;
            th.Start();

            return await Task.Run(() =>
            {
                while (th.IsAlive)
                    Thread.Sleep(10);
                return token;
            });
        }
    }
}
