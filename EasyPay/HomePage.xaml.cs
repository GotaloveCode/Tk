using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (localSettings.Values["PhoneNo"]!=null)
            txtPhoneNo.Text = localSettings.Values["PhoneNo"].ToString();
        }


        private void btnGenerateCode_Click(object sender, RoutedEventArgs e)
        {
            //store phoneNo
            localSettings.Values["PhoneNo"] = txtPhoneNo.Text;

            IBarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,//Mentioning type of bar code generation   
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 300,
                    Width = 300
                },
                Renderer = new ZXing.Rendering.PixelDataRenderer() { Foreground = Colors.Black,Background = Colors.LightBlue }//Adding color QR code   
            };
            var result = writer.Write(txtPhoneNo.Text);
            var wb = result.ToBitmap() as WriteableBitmap;
            //Displaying QRCode Image   
            QRCode.Source = wb;   
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
                btnGenerateCode.IsEnabled = true;
            else
                btnGenerateCode.IsEnabled = false;
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

       




    }
}
