using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace TekeTeke
{

    public sealed partial class SignUpPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        ResourceLoader res = new ResourceLoader();
        HttpStatusCode statuscode;
        JObject output;
        public string Code { get; set; }

        // temporary variable for holding the latest textBox value before the textChange event is trigerred
        string txtTemp = "";
        // this boolean is to be used by the textChanged event to decide to accept changes or not
        bool acceptChange = true;
        public SignUpPage()
        {
            this.InitializeComponent();
        }


        private async void btnVerify_Click(object sender, RoutedEventArgs e)
        {
            await Verify();
        }
        //PhoneNo Events
        #region
        private void txtPhoneNo_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //whenever a key is pressed, capture the latest textBox value
            txtTemp = txtPhoneNo.Text;
        }

        private void txtPhoneNo_GotFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived += inputEntered;
        }

        private void txtPhoneNo_LostFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived -= inputEntered;
        }

        private void inputEntered(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            // reset the bool to true in case it was set to false in the last call
            acceptChange = true;           
            args.Handled = true;

            //in my case I needed only numeric value and the backSpace button 
            if ((args.KeyCode > 47 && args.KeyCode < 58) || args.KeyCode == 8)
            {
                //do nothing (i.e. acceptChange is still true)
            }
            else
            {
                //set acceptChange to false bec. character is not numeric nor backSpace
                acceptChange = false;
            }
        }

        private void txtPhoneNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPhoneNo.Text.Length < 13)
            {
                if (!acceptChange)
                {
                    txtPhoneNo.Text = txtTemp;
                    //this is to move the cursor to the end of the text in the textBox
                    txtPhoneNo.Select(txtPhoneNo.Text.Length, 0);
                }
            }
            else
            {
                txtPhoneNo.Text = txtTemp;
                //this is to move the cursor to the end of the text in the textBox
                txtPhoneNo.Select(txtPhoneNo.Text.Length, 0);
            }
        }

        #endregion

        private async Task Verify()
        {

            if (string.IsNullOrEmpty(txtPhoneNo.Text) || txtPhoneNo.Text.Length < 12)
            {
                await UIHelper.ShowAlert(res.GetString("Enterphone"));

            }
            else
            {
                //check internet
                if (UIHelper.HasInternetConnection())
                {
                    await VerifyAsync();
                }
                else
                {
                    await UIHelper.ShowAlert(res.GetString("NoInternet"));
                }
            }
        }

        private async Task VerifyAsync()
        {
            await UIHelper.ToggleProgressBar(true, res.GetString("Loading"));

            JsonObject input = new JsonObject();

            var MyResult = await Rest.GetAsync("mpesa/public/verify-phone?phone_number=" + txtPhoneNo.Text, "");

            statuscode = MyResult.Key;

            output = MyResult.Value;

            await UIHelper.ToggleProgressBar(false);

            if (statuscode == HttpStatusCode.OK)
            {
                if ((string)output["status"] != "error")
                {
                    Code = (string)output["verification_code"];
                    await UIHelper.ShowAlert((string)output["message"]);
                }
                else
                {
                    await UIHelper.ShowAlert((string)output["error"]["message"][0]);
                }
            }
            else
            {
                await UIHelper.ShowAlert((string)output["error"]["message"][0]);
            }
            input.Clear();
        }

        private async Task Register()
        {
            ErrorBucket errors = new ErrorBucket();
            ValidateSignUp(errors);
            if (!(errors.HasErrors))
            {
                localSettings.Values["Phone"] = txtPhoneNo.Text;
                //Frame rootFrame = Window.Current.Content as Frame;
                //rootFrame = new Frame();
                this.Frame.Navigate(typeof(HomePage), txtPhoneNo.Text);
            }
            else
            {
                await UIHelper.ShowAlert(errors.GetErrorsAsString());
                errors.ClearErrors();
            }
        }
     
        //validate register
        private void ValidateSignUp(ErrorBucket errors)
        {
         
            if (string.IsNullOrEmpty(txtPhoneNo.Text) || txtPhoneNo.Text.Length < 12)
                errors.AddError(res.GetString("InvalidPhone"));

            if (string.IsNullOrEmpty(txtVerify.Text))
                errors.AddError(res.GetString("EnterReceived"));
            
            if (!string.IsNullOrEmpty(txtVerify.Text) && txtVerify.Text !=Code)
                errors.AddError(res.GetString("InvalidCode"));

        }

        private async void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            await Register();
        }
    }
}
