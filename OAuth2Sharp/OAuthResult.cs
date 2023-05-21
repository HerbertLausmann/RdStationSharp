using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2Sharp
{
    /// <summary>
    /// Represents the result of an OAuth authentication process.
    /// </summary>
    public class OAuthResult
    {
        private string _token;
        private OAuthResultType _result;

        /// <summary>
        /// Gets the OAuth token if the authentication is successful, otherwise null.
        /// </summary>
        public string Token => _token;

        /// <summary>
        /// Gets the OAuthResultType indicating the outcome of the authentication process.
        /// </summary>
        public OAuthResultType Result => _result;

        /// <summary>
        /// Gets a value indicating whether the authentication process has succeeded.
        /// </summary>
        public bool HasSucceeded => _result == OAuthResultType.Success;

        /// <summary>
        /// Initializes a new instance of the OAuthResult class with the specified token and result type.
        /// </summary>
        /// <param name="token">The OAuth token as a string.</param>
        /// <param name="result">The OAuthResultType indicating the outcome of the authentication process.</param>
        internal OAuthResult(string token, OAuthResultType result)
        {
            _token = token;
            _result = result;
        }
    }

    /// <summary>
    /// Represents the possible outcomes of an OAuth authentication process.
    /// </summary>
    public enum OAuthResultType
    {
        /// <summary>
        /// The authentication process has succeeded and the OAuth token is available.
        /// </summary>
        Success,

        /// <summary>
        /// The authentication process has failed due to an error.
        /// </summary>
        Failed,

        /// <summary>
        /// The authentication process was cancelled by the user.
        /// </summary>
        Cancelled
    }
}
