using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;



namespace TekeTeke
{

    public sealed partial class HomePage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder folder = ApplicationData.Current.LocalFolder;
        CultureInfo culture = CultureInfo.CurrentCulture;

        public string Code { get; set; }

        public string countryCode { get; set; }

        // temporary variable for holding the latest textBox value before the textChange event is trigerred
        string txtTemp = "";
        // this boolean is to be used by the textChanged event to decide to accept changes or not
        bool acceptChange = true;
        public HomePage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Required;
        }


        private void txtPhoneNo_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //whenever a key is pressed, capture the latest textBox value
            txtTemp = txtPhoneNo.Text;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //clear verification code
            txtVerify.Text = "";
            if (localSettings.Values["PhoneNo"] != null)
            {
                string phonenumber = localSettings.Values["PhoneNo"].ToString();

                if (localSettings.Values["countryCode"] != null)
                    cmbCode.SelectedItem = localSettings.Values["countryCode"].ToString();

                //retrieve image
                string fileName = "MyQr.jpg";

                try
                {
                    StorageFile file = await folder.GetFileAsync(fileName);
                    var image = await FileIO.ReadBufferAsync(file);
                    Uri uri = new Uri(file.Path);
                    BitmapImage img = new BitmapImage(new Uri(file.Path));
                    QRCode.Source = img;
                }
                catch (FileNotFoundException)
                {
                    if (culture.Name == "fr")
                        await UIHelper.ShowAlert("Image QR manquant"); 
                    else
                        await UIHelper.ShowAlert("Missing QR Image");
                }

            }
            else
            {
                btnGenerateCode.IsEnabled = true;
            }


        }


        private async void btnVerifyCode_Click(object sender, RoutedEventArgs e)
        {
            if (!Code.Equals(txtVerify.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                if (culture.Name == "fr")
                    await UIHelper.ShowAlert("Code invalide"); 
                else
                    await UIHelper.ShowAlert("Invalid code");
            }
            else
            {


                IBarcodeWriter writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,//Mentioning type of bar code generation   
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 300,
                        Width = 300
                    },
                    Renderer = new ZXing.Rendering.PixelDataRenderer() { Foreground = Colors.Black, Background = Colors.Silver }//Adding color QR code   
                };

                ComboBoxItem typeItem = (ComboBoxItem)cmbCode.SelectedItem;

                string countryCode = typeItem.Content.ToString();

                var result = writer.Write(countryCode + txtPhoneNo.Text);

                var wb = result.ToBitmap() as WriteableBitmap;

                //Displaying QRCode Image   
                QRCode.Source = wb;
                if (culture.Name == "fr")
                    await UIHelper.ShowAlert("QR généré avec succès"); 
                else
                    await UIHelper.ShowAlert("QR Generated Successfully");

                //empty & hide verify text field,hide verify button  
                txtVerify.Text = "";
                txtVerify.Visibility = Visibility.Collapsed;
                btnVerifyCode.Visibility = Visibility.Collapsed;
                btnGenerateCode.IsEnabled = false;
                //store phoneNo
                localSettings.Values["PhoneNo"] = txtPhoneNo.Text;
                localSettings.Values["countryCode"] = countryCode;

                if (folder != null)
                {
                    StorageFile file = await folder.CreateFileAsync("MyQr.jpg", CreationCollisionOption.ReplaceExisting);

                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                        Stream pixelStream = wb.PixelBuffer.AsStream();
                        byte[] pixels = new byte[pixelStream.Length];
                        await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)wb.PixelWidth, (uint)wb.PixelHeight, 48, 48, pixels);
                        await encoder.FlushAsync();
                    }
                }

            }
        }

        private void txtPhoneNo_GotFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived += inputEntered;
        }

        private void txtPhoneNo_LostFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived -= inputEntered;
        }

        // here we recieve the character and decide to accept it or not.
        private void inputEntered(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            // reset the bool to true in case it was set to false in the last call
            acceptChange = true;

            //enable btngenerate
            if (txtPhoneNo.Text.Length == 9)
            {
                btnGenerateCode.IsEnabled = true;

            }
            else
            {
                btnGenerateCode.IsEnabled = false;
            }

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
            if (btnGenerateCode.Visibility == Visibility.Collapsed)
                btnGenerateCode.Visibility = Visibility.Visible;
            if (txtPhoneNo.Text.Length < 10)
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


        private async void btnGenerateCode_Click(object sender, RoutedEventArgs e)
        {
            //no internet
            if (NetworkInformation.GetInternetConnectionProfile() == null)
            {
                if (culture.Name == "fr")
                    await UIHelper.ShowAlert("Vérifier votre connexion internet", "Pas de connectivité internet");
                else
                    await UIHelper.ShowAlert("Check your internet connection", "No internet connectivity");
            }
            else
            {
                if (txtPhoneNo.Text.Length != 9 || !IsDigitsOnly(txtPhoneNo.Text))
                {
                    if (culture.Name == "fr")
                        await UIHelper.ShowAlert("Format du numéro de téléphone doit être 254712345678 ou 243712345678", "Numéro de téléphone non valide");
                    else
                        await UIHelper.ShowAlert("Phone number format should be 254712345678", "Invalid phone number");
                }

                else
                {
                    await PostAsync();

                }

            }

        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }


        private async Task PostAsync()
        {
            ComboBoxItem typeItem = (ComboBoxItem)cmbCode.SelectedItem;

            countryCode = typeItem.Content.ToString();

            string url = "http://139.59.186.10/mpesa/public/verify-phone?phone_number=" + countryCode + txtPhoneNo.Text;

            // client...
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if (culture.Name == "fr")
                UIHelper.ToggleProgressBar(true, "Vérification du code du serveur..");
            else
                UIHelper.ToggleProgressBar(true, "Verifying code from server..");

            HttpResponseMessage response = await client.GetAsync(url);

            // load it up...
            string outputJson = await response.Content.ReadAsStringAsync();

            JObject output = JObject.Parse(outputJson);
            if (response.StatusCode == HttpStatusCode.OK)
            {

                String status = (string)output["status"];
                if (status != "error")
                {
                    Code = (string)output["verification_code"];

                    if (culture.Name == "fr")
                        await UIHelper.ShowAlert("Entrer le code reçu");
                    else
                        await UIHelper.ShowAlert("Enter Received Code");
                    //make textbox for code visible
                    txtVerify.Visibility = Visibility.Visible;
                    btnVerifyCode.Visibility = Visibility.Visible;
                    txtVerify.Focus(FocusState.Keyboard);
                    //ideally we would hide this but since server fails at times
                    //btnGenerateCode.Visibility = Visibility.Collapsed;
                }
                else
                {
                    string error = (string)output["error"]["code"];
                    await UIHelper.ShowAlert(error);
                }

            }
            else
            {
                string error = (string)output["error"]["message"][0];
                await UIHelper.ShowAlert(error);
            }
            UIHelper.ToggleProgressBar(false);

        }



    }

    class PhoneQRs
    {
        public string UserType { get; set; }
        public string PhoneNo { get; set; }
    }
}
