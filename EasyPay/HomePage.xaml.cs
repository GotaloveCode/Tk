using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;



namespace EasyPay
{

    public sealed partial class HomePage : Page
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        List<PhoneQRs> QrList = new List<PhoneQRs>();
        public string Code { get; set; }

        // temporary variable for holding the latest textBox value before the textChange event is trigerred
        string txtTemp = "";
        // this boolean is to be used by the textChanged event to decide to accept changes or not
        bool acceptChange = true;
        public HomePage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
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
                txtPhoneNo.Text = localSettings.Values["PhoneNo"].ToString(); 
                //retrieve image
                string fileName = "MyQr.png";
                StorageFolder myfolder = ApplicationData.Current.LocalFolder;
                BitmapImage bitmapImage = new BitmapImage(); 
                try
                {
                    StorageFile file = await myfolder.GetFileAsync(fileName);
                    var image = await Windows.Storage.FileIO.ReadBufferAsync(file);
                    Uri uri = new Uri(file.Path);
                    BitmapImage img = new BitmapImage(new Uri(file.Path));
                    QRCode.Source = img;
                }
                catch (FileNotFoundException)
                {
                    UIHelper.ShowAlert("Missing QR Image");
                }
              
            }
            else
            {
                btnVerify.IsEnabled = true;
            }
           

        }


        private async void btnGenerateCode_Click(object sender, RoutedEventArgs e)
        {
            if (!Code.Equals(txtVerify.Text,StringComparison.CurrentCultureIgnoreCase))
            {
                await UIHelper.ShowAlert("Invalid code");
            }
            else
            {
                //store phoneNo
               // localSettings.Values["PhoneNo"] = txtPhoneNo.Text;

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
                var result = writer.Write(txtPhoneNo.Text);
                var wb = result.ToBitmap() as WriteableBitmap;

                //Displaying QRCode Image   
                QRCode.Source = wb;
                await UIHelper.ShowAlert("QR Generated Successfully");
                //empty verify text field
                txtVerify.Text = "";
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                if (folder != null)
                {
                    StorageFile file = await folder.CreateFileAsync("MyQr" + ".png", CreationCollisionOption.ReplaceExisting);
                    using (var storageStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, storageStream);
                        var pixelStream = wb.PixelBuffer.AsStream();
                        var pixels = new byte[pixelStream.Length];
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
            if (txtPhoneNo.Text.Length == 12)
            {
                btnVerify.IsEnabled = true;
               
            }
            else
            {
                btnVerify.IsEnabled = true;
            }
            if (!txtPhoneNo.Text.StartsWith("254"))
            {
                txtPhoneNo.Text = "254";
            }  

            System.Diagnostics.Debug.WriteLine("KeyPress " + Convert.ToChar(args.KeyCode) + "keyCode = " + args.KeyCode.ToString());
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
            if(btnVerify.Visibility == Visibility.Collapsed)
                btnVerify.Visibility = Visibility.Visible;
            if (txtPhoneNo.Text.Length < 13)
            {
                if (acceptChange)
                {
                    // do nothing
                }
                else
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


        private async void btnVerify_Click(object sender, RoutedEventArgs e)
        {
            if (txtPhoneNo.Text.Length != 12 || !IsDigitsOnly(txtPhoneNo.Text))
            {
                await UIHelper.ShowAlert("Invalid Phone number");
            }
            else
            {
                await PostAsync();
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
            var url = "http://139.59.186.10/mpesa/public/verify-phone?phone_number=" + txtPhoneNo.Text;       

            // client...
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            UIHelper.ToggleProgressBar(true, "Verifying code from server..");
            var response = await client.GetAsync(url);

            // load it up...
            var outputJson = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject output = JObject.Parse(outputJson);
                String status = (string)output["status"];
                if (status != "error")
                {                    
                    Code = (string)output["verification_code"]; 
                    await UIHelper.ShowAlert("Enter Received Code");
                    btnVerify.Visibility = Visibility.Collapsed;
                    txtVerify.Focus(FocusState.Keyboard);
                }
                else
                {
                    string error = (string)output["error"]["code"];
                    await UIHelper.ShowAlert(error);
                }
               
            }
            UIHelper.ToggleProgressBar(false);

        }

        private void txtVerify_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(btnGenerateCode.IsEnabled == false)
             btnGenerateCode.IsEnabled = true;
        }

        

    }

    class PhoneQRs
    {
        public string UserType { get; set; }
        public string PhoneNo { get; set; }
    }
}
