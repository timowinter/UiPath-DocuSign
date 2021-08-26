using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;
using DocuSign.eSign.Model;
using System.IO;

namespace DocumentSign.Models
{
    /// <summary>
    /// Authentication Class for DocuSign Platform provides Methods and Properties to authenticate at DocuSign
    /// For further Information check: https://developers.docusign.com/esign-rest-api/guides/authentication/oauth2-jsonwebtoken
    /// </summary>
    public class Authentication
    {
        /// <summary>
        /// Constant Value for Token replacement
        /// </summary>
        private const int TOKEN_REPLACEMENT_IN_SECONDS = 10 * 60;

        /// <summary>
        /// Property for Access Token
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// Property for ClientID
        /// </summary>
        protected string ClientID { get; set; }

        /// <summary>
        /// Property for personder UserID
        /// </summary>
        protected string ImpersonatedUserGuid { get; set; }

        /// <summary>
        /// Property for Authentication Server (usally Productive and Demo System available)
        /// </summary>
        protected string AuthServer { get; set; }

        /// <summary>
        /// RSA Private Key in Format of PEM Base64 like:
        /// -----BEGIN PRIVATE KEY-----
        ///MIIBgjAcBgoqhkiG9w0BDAEDMA4ECKZesfWLQOiDAgID6ASCAWBu7izm8N4V   
        ///..
        ///-----END PRIVATE KEY----
        /// </summary>
        protected string PrivateKey { get; private set; }

        /// <summary>
        /// Redirect URL
        /// </summary>
        string RedirectUrl { get; set; }

        /// <summary>
        /// Target Account ID
        /// </summary>
        string TargetAccountID { get; set; }

        /// <summary>
        /// Variable for Acces Token expiry
        /// </summary>
        private static int expiresIn;

        /// <summary>
        /// Account Instance
        /// </summary>
        private static Account Account { get; set; }

        /// <summary>
        /// Api Property
        /// </summary>
        public ApiClient ApiClient { get; private set; }

        /// <summary>
        /// AccountID will be accessed through instance of Account
        /// </summary>
        public string AccountID
        {
            get { return Account.AccountId; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ClientID"></param>
        /// <param name="ImpersonatedUserGuid"></param>
        /// <param name="AuthServer"></param>
        /// <param name="PrivateKey"></param>
        /// <param name="TargetAccountID"></param>
        /// <param name="Redirect"></param>
        public Authentication(ApiClient client, string ClientID, string ImpersonatedUserGuid, string AuthServer, string PrivateKey, string TargetAccountID = "FALSE", string Redirect = "http://localhost/")
        {
            ApiClient = client;
            this.PrivateKey = PrivateKey;
            this.ClientID = ClientID;
            this.ImpersonatedUserGuid = ImpersonatedUserGuid;
            this.AuthServer = AuthServer;
            this.TargetAccountID = TargetAccountID;
            this.RedirectUrl = Redirect;
        }

        /// <summary>
        /// Null Check Token and ExpiryDate Check
        /// </summary>
        public void CheckToken()
        {
            if (AccessToken == null
                || (DateTime.Now.Millisecond + TOKEN_REPLACEMENT_IN_SECONDS) > expiresIn)
            {
                UpdateToken();
            }
        }

        /// <summary>
        /// Request a fresh Access Token from DocuSign Platform
        /// </summary>
        private void UpdateToken()
        {
            try
            {
                //Request Json Web Token with given DocuSign Information
                OAuth.OAuthToken authToken = ApiClient.RequestJWTUserToken(ClientID,
                            ImpersonatedUserGuid,
                            AuthServer,
                            Encoding.UTF8.GetBytes(PrivateKey),
                            1);

                //Store Access Token
                AccessToken = authToken.access_token;

                if (Account == null)
                    Account = GetAccountInfo(authToken);

                //Define RestAPI as Endpoint
                ApiClient = new ApiClient(Account.BaseUri + "/restapi");

                //For further use if neccessary
                expiresIn = DateTime.Now.Second + authToken.expires_in.Value;

                Console.WriteLine("Access Token expiresIn: " + expiresIn.ToString() + "s");

            }
            catch (ApiException e)
            {
                Console.WriteLine("\nDocuSign Exception!");

                // Special handling for consent_required
                String message = e.Message;
                if (!String.IsNullOrWhiteSpace(message) && message.Contains("consent_required"))
                {
                    String consent_url = String.Format("\n    {0}/oauth/auth?response_type=code&scope={1}&client_id={2}&redirect_uri={3}",
                        AuthServer, "signature impersonation", ClientID, RedirectUrl);

                    Console.WriteLine("C O N S E N T   R E Q U I R E D");
                    Console.WriteLine("Ask the user who will be impersonated to run the following url: ");
                    Console.WriteLine(consent_url);
                    Console.WriteLine("\nIt will ask the user to login and to approve access by your application.");
                    Console.WriteLine("Alternatively, an Administrator can use Organization Administration to");
                    Console.WriteLine("pre-approve one or more users.");
                }
                else
                    Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Retrieve account information for given Access Token
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        private Account GetAccountInfo(OAuth.OAuthToken authToken)
        {
            //Set up BaseBath
            ApiClient.SetOAuthBasePath(AuthServer);

            //Get User Info by AccesToken
            OAuth.UserInfo userInfo = ApiClient.GetUserInfo(authToken.access_token);
            Account acct = null;

            var accounts = userInfo.Accounts;

            if (!string.IsNullOrEmpty(TargetAccountID) && !TargetAccountID.Equals("FALSE"))
            {
                acct = accounts.FirstOrDefault(a => a.AccountId == TargetAccountID);

                if (acct == null)
                {
                    throw new Exception("The user does not have access to account " + TargetAccountID);
                }
            }
            else
            {
                acct = accounts.FirstOrDefault(a => a.IsDefault == "true");
            }

            return acct;
        }

    }
}
