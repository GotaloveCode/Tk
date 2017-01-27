using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TekeTeke
{
    public class Rest
    {
        internal const string ServiceUrlBase = "http://139.59.186.10/";//

        // holds a reference to the logon token...
        public static string Token { get; set; }

        public static bool HasLogonToken
        {
            get
            {
                return !(string.IsNullOrEmpty(Token));
            }
        }


        public static async Task<KeyValuePair<HttpStatusCode, JObject>> PostAsync(string Url, string jsonstr)
        {
            HttpClient client = new HttpClient();

            if (HasLogonToken)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            }
            //no caching
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;

            StringContent content = new StringContent(jsonstr, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ServiceUrlBase + Url, content);

            // load it up...
            string outputJson = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(Url);
            Debug.WriteLine(outputJson);
            JObject output = JObject.Parse(outputJson);
            ResourceLoader res = ResourceLoader.GetForCurrentView();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                outputJson = "{\"status\":\"error\"," +
                             "\"error\":{" +
                                "\"code\":\"404\"," +
                                "\"message\":[\"" + res.GetString("NoInternet") + "\"]" +
                                 "}}";
            }
            //else if (response.StatusCode == HttpStatusCode.Unauthorized && Url != "logout")
            //{
            //    try
            //    {
            //        //this can only happen if app is logged in elsewhere
            //        if ((string)output["message"] == "Token has expired")
            //        {
            //            ApplicationData.Current.LocalSettings.Values.Remove("Token");
            //            await UIHelper.ShowAlert(res.GetString("ExpiredSession"));
            //            Frame rootFrame = Window.Current.Content as Frame;
            //            rootFrame.Navigate(typeof(SignUpPage));
            //        }
            //        else
            //        {
            //            await UIHelper.ShowAlert(res.GetString("IncorrectCredentials"));
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        //handle gracefully incase token is jarray 
            //    }
            //}

            return new KeyValuePair<HttpStatusCode, JObject>(response.StatusCode, output);
        }

        //Get methods
        public static async Task<KeyValuePair<HttpStatusCode, JObject>> GetAsync(string Url, string myparams)
        {
            HttpClient client = new HttpClient();

            if (HasLogonToken)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            }

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //no caching
            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Now;

            Url = ServiceUrlBase + Url + myparams;

            HttpResponseMessage response = await client.GetAsync(Url);            

            string outputJson = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(Url);
            Debug.WriteLine(outputJson);
            JObject output = JObject.Parse(outputJson);
            ResourceLoader res = ResourceLoader.GetForCurrentView();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {

                outputJson = "{\"status\":\"error\"," +
                             "\"error\":{" +
                                "\"code\":\"404\"," +
                                "\"message\":[\"" + res.GetString("NoInternet") + "\"]" +
                                 "}}";
            }
            //else if (response.StatusCode == HttpStatusCode.Unauthorized)
            //{
            //    try
            //    {
            //        //this can only happen if app is logged in elsewhere
            //        if ((string)output["message"] == "Token has expired")
            //        {
            //            ApplicationData.Current.LocalSettings.Values.Remove("Token");
            //            await UIHelper.ShowAlert(res.GetString("ExpiredSession"));
            //            Frame rootFrame = Window.Current.Content as Frame;
            //            rootFrame.Navigate(typeof(PinPage));
            //        }
            //        else
            //        {
            //            await UIHelper.ShowAlert(res.GetString("IncorrectCredentials"));
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        //handle gracefully incase token is jarray 
            //    }
            //}

            return new KeyValuePair<HttpStatusCode, JObject>(response.StatusCode, output);
        }

        public static async Task LogOutAsync()
        {
            ResourceLoader res = ResourceLoader.GetForCurrentView();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (UIHelper.HasInternetConnection())
            {
                await UIHelper.ToggleProgressBar(true, res.GetString("SignOut"));

                var MyResult = await PostAsync("logout", "");

                HttpStatusCode statuscode = MyResult.Key;

                JObject output = MyResult.Value;

                Debug.WriteLine(statuscode + " " + (string)output["message"]);

                Frame rootFrame = Window.Current.Content as Frame;

                if (statuscode == HttpStatusCode.OK)
                {
                    //remove Token as is done
                    ApplicationData.Current.LocalSettings.Values.Remove("Token");
                    //remove LoggedIn token
                    ApplicationData.Current.LocalSettings.Values.Remove("LoggedIn");
                    rootFrame.Navigate(typeof(SignUpPage));
                }
                else
                {
                    string error = (string)output["message"];
                    if (error == "The token has been blacklisted")
                        ApplicationData.Current.LocalSettings.Values.Remove("Token");
                    rootFrame.Navigate(typeof(SignUpPage));
                }

                await UIHelper.ToggleProgressBar(false);
            }
            else
            {
                await UIHelper.ShowAlert(res.GetString("NoInternet"));
            }

        }



    }
}
