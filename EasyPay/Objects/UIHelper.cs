using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace EasyPay
{
    public static class UIHelper
    {
        public static async void ToggleProgressBar(bool toggle, string message = "")
        {
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

            if (toggle)
            {
                statusBar.ProgressIndicator.Text = message;
                await statusBar.ProgressIndicator.ShowAsync();
            }
            else
            {
                await statusBar.ProgressIndicator.HideAsync();
            }
        }

        public static async Task ShowAlert(string message, string title = "")
        {
            MessageDialog dialog = new MessageDialog(message, title);
            await dialog.ShowAsync();
        }
    }
}
