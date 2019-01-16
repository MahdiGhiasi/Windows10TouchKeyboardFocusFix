using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Windows10TouchKeyboardFocusFix
{
    internal static class GoogleAnalyticsHelper
    {
        private static Random random;
        private static string trackingId;
        private static string trackingDomain;
        private static HttpClient httpClient;

        static GoogleAnalyticsHelper()
        {
            random = new Random();
            trackingId = Secrets.AnalyticsTrackingCode;
            trackingDomain = Secrets.AnalyticsDomain;

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.UserId))
            {
                // Create random user id
                Properties.Settings.Default.UserId = Guid.NewGuid().ToString();
                Properties.Settings.Default.Save();
            }
        }

        internal static async void TrackPage(string pageName)
        {
#if !DEBUG
            try
            {
                await TrackPage(Properties.Settings.Default.UserId, pageName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TrackPage failed: " + ex.ToString());
            }
#endif
        }

        internal static async void TrackEvent(string eventCategory, string eventAction, string eventLabel = "", int? eventValue = null)
        {
#if !DEBUG
            try
            {
                var eventValueString = (eventValue == null) ? "" : eventValue.ToString();
                await TrackEvent(Properties.Settings.Default.UserId,
                    eventCategory, eventAction, eventLabel, eventValueString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TrackEvent failed: " + ex.ToString());
            }
#endif
        }

        private static async Task TrackEvent(string userId, string eventCategory, string eventAction, string eventLabel = "", string eventValue = "")
        {
            string url = $"https://www.google-analytics.com/collect?v=1&" +
                $"tid={WebUtility.UrlEncode(trackingId)}&t=event" +
                $"&ul=en-us&ec={WebUtility.UrlEncode(eventCategory)}&ea={WebUtility.UrlEncode(eventAction)}" +
                $"&el={WebUtility.UrlEncode(eventLabel)}&ev={WebUtility.UrlEncode(eventValue)}" +
                $"&cid={WebUtility.UrlEncode(userId)}&z={random.Next()}";

            await SendRequest(url);
        }

        private static async Task TrackPage(string userId, string pageName)
        {
            pageName += "/";
            string url = $"https://www.google-analytics.com/collect?v=1&" +
                $"tid={WebUtility.UrlEncode(trackingId)}&t=pageview" +
                $"&ul=en-us&dh={WebUtility.UrlEncode(trackingDomain)}&dp={WebUtility.UrlEncode("/" + pageName)}" +
                $"&cid={WebUtility.UrlEncode(userId)}&z={random.Next()}";

            await SendRequest(url);
        }

        private static async Task SendRequest(string url)
        {
            if (httpClient == null)
                httpClient = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, url);
            await httpClient.SendAsync(message);
        }
    }
}
