using System.Windows;

namespace INTERFACE_POSTRATA.Services
{
    public static class DialogService
    {
        public static void Info(string message, string title = "Informação")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Warn(string message, string title = "Aviso")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Error(string message, string title = "Erro")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
