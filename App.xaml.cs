using System;
using System.Windows;

namespace WpfProjectCleanerMvvmNet5
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //  Trace.WriteLine(string.Format("{0}: Error: {1}", DateTime.Now, e.Exception));
            MessageBox.Show("Error encountered! Please contact support." +
                            Environment.NewLine + e.Exception.Message, "Exception Error");
            Shutdown(1);
            e.Handled = true;
        }
    }


}
