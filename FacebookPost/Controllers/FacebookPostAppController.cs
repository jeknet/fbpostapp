using FacebookPost.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FacebookPost.Controllers
{
    public class FacebookPostAppController : Controller
    { 
        public ActionResult Index()
        {
            if (!ValidateRequest(Request))
            {
                return RequestAppKey();
            }
            if (ExistValidTokenForApp(Request.Params["AppKey"]))
            { 
                return PostToFacebook();
            }
            else { 
                return RequestPermissions(Request.Params["AppKey"]);
            } 
        }

        public RedirectResult RequestPermissions(string appKey) {
            Session["AppKey"] = appKey;
            string url = string.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri=http://fbloginjosue.azurewebsites.net/FacebookPostApp/TokenAquired&scope=publish_actions&response_type=token", appKey);
            return Redirect(url);
        }

        public ActionResult RequestAppKey() {
            return View("RequestAppKey");
        }

        public ActionResult PostToFacebook()
        {
            return View("PostToFacebook");
        }

        public ActionResult TokenAquired() {
            return View("TokenAquired");
        }

        public ActionResult TokenAquiredAndProcessed() { 
        string access_token = Request.Params["access_token"];
            string appKey = Session["AppKey"] as string;
            string expirationTime = Request.Params["expires_in"];
            CreateToken(access_token, appKey, Int32.Parse(expirationTime));

            return View("PostToFacebook");
        }

        public ActionResult PostCompleted() {
            return View("PostCompleted");
        }

        private void CreateToken(string access_token, string appKey, int expirationTime)
        {
            FacebookToken token = new FacebookToken();
            token.ApiKey = appKey;
            token.creationTime = DateTime.Now;
            token.TokenExpirationTime = expirationTime;
            token.Token = access_token;

            AddToken(token);
        }

        [HttpPost]
        public ActionResult PostToFacebook(FacebookPostValue post)
        {
            string postString = Request.Params["Post"];

            FacebookToken token = GetToken(Session["AppKey"] as string);
            string url = string.Format("https://graph.facebook.com/josue.delDF/feed?access_token={0}", token.Token);
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";

            using (Stream requestStream = request.GetRequestStream()) {
                String message = string.Format("message=\"{0}\"", Request.Params["Post"]);
                byte[] messageArray = Encoding.UTF8.GetBytes(message);
                int length = messageArray.Length;
                requestStream.Write(messageArray, 0, length);
            } 
             
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Console.WriteLine(response.StatusCode);

            return View("PostCompleted");
        }

        private bool ExistValidTokenForApp(string appKey)
        {
            IDictionary<string, FacebookToken> tokens = GetTokens();

            if (!tokens.ContainsKey(appKey))
            {
                return false;
            }
            Session["AppKey"] = appKey;

            DateTime now = DateTime.Now;
            FacebookToken token = tokens[appKey];

            DateTime expirationDate = token.creationTime.AddSeconds(token.TokenExpirationTime);
            int testVal = expirationDate.CompareTo(now);
          
            return testVal > 0; 
        }

        private IDictionary<string, FacebookToken> GetTokens() {
            IDictionary<string, FacebookToken> tokens = Session["facebookTokens"] as IDictionary<string, FacebookToken>;

            if (tokens == null) {
                tokens = new Dictionary<string, FacebookToken>();
                Session["facebookTokens"] = tokens;
            }

            return tokens;
        }
        private void AddToken(FacebookToken token) {
            IDictionary<string, FacebookToken> tokens = GetTokens();

            if (tokens.ContainsKey(token.ApiKey)) {
                tokens.Remove(token.ApiKey);
            }

            tokens.Add(token.ApiKey, token);

            Session["facebookTokens"] = tokens;

        }
        private FacebookToken GetToken(string appKey) {
            IDictionary<string, FacebookToken> tokens = GetTokens();

            return tokens[appKey];
        }

        private bool ValidateRequest(HttpRequestBase Request)
        {
           return !string.IsNullOrEmpty(Request.Params["AppKey"]);
        }

    }
}
