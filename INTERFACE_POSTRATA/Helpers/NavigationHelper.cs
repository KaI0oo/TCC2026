using System.Linq;
using System.Windows;

namespace INTERFACE_POSTRATA.Helpers
{
    public static class NavigationHelper
    {
        public static void ShowMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existing = Application.Current.Windows.OfType<Window1>().FirstOrDefault();
                if (existing != null)
                {
                    if (!existing.IsVisible) existing.Show();
                    existing.Activate();
                }
                else
                {
                    var w = new Window1();
                    w.Show();
                }
            });
        }
    }
}
