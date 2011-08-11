using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Hammock;
using Hammock.Authentication.Basic;

namespace BatchedTweets.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Tweets()
        {
            return View();
        }

        public JsonResult TweetsGet()
        {
            var timeRemaining = TimeUntilMoreTweetsAreAllowed();
            if (timeRemaining > 0) return Json(timeRemaining, JsonRequestBehavior.AllowGet);

            var content = RetrieveTweets(MostRecentTweet);

            var x = new JavaScriptSerializer().Deserialize<dynamic>(content);
            MostRecentTweet = x[0]["id"].ToString();
            Session.Add("LastHit", DateTime.Now);

            return  Json(content, JsonRequestBehavior.AllowGet);
        }

        private string MostRecentTweet
        {
            get { return Session["since_id"] == null ? "" : Session["since_id"].ToString(); }
            set { Session.Add("since_id", value);}
        }

        private static string RetrieveTweets(string sinceId)
        {
            BasicAuthCredentials credentials = new BasicAuthCredentials
                                                   {
                                                       Username = "zdam",
                                                       Password = "sambooka"
                                                   };
            RestClient client = new RestClient
                                    {
                                        Authority = "http://api.supertweet.net",
                                        VersionPath = "1"
                                    };

            string possibleSinceId = string.IsNullOrWhiteSpace(sinceId) ? "": string.Format("&since_id={0}", sinceId) ;

            RestRequest request = new RestRequest
                                      {
                                          Credentials = credentials,
                                          Path = string.Format("statuses/home_timeline.json?count=200&include_entities=false{0}", possibleSinceId)
                                      };
            RestResponse response = client.Request(request);

            var content = response.Content;
            return content;
        }

        private int TimeUntilMoreTweetsAreAllowed()
        {
            var lastHit = Session["LastHit"];
            if (lastHit == null)
            {
                Session.Add("LastHit", DateTime.Now);
                return 0;
            }

            var lastHitDate = Convert.ToDateTime(lastHit);
            return 3 - Convert.ToInt32(DateTime.Now.Subtract(lastHitDate).TotalMinutes);
        }
    }
}
