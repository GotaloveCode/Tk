using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;



namespace TekeTeke
{

    public sealed partial class HomePage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
 
        public HomePage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (localSettings.Values["Phone"] != null)
            {
                IBarcodeWriter writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,//Mentioning type of bar code generation   
                    Options = new ZXing.Common.EncodingOptions{ Height = 280,Width = 280},
                    Renderer = new ZXing.Rendering.PixelDataRenderer() { Foreground = Colors.Black, Background = Colors.Transparent }//Adding color QR code   
                };
                var result = writer.Write(localSettings.Values["Phone"].ToString());
                var wb = result.ToBitmap() as WriteableBitmap;
                //Displaying QRCode Image   
                QRCode.Source = wb;
            }
        }

     

    }

   
}
