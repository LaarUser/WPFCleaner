using System.Windows;

namespace WpfProjectCleanerMvvmNet5
{
    /// <summary>
    /// Interaction logic for AddExtension.xaml
    /// </summary>
    public class MyExtension : ViewModelBase
    {
        private string _fileExtension=string.Empty;
        public string FileExtension { get => _fileExtension;  set{ _fileExtension=value;OnPropertyChanged();}  }
    }

    public partial class AddExtension : Window
    {
       private MyExtension dc;
        public AddExtension()
        {
            InitializeComponent();
            dc = new MyExtension();
            dc.FileExtension = ".ext";

            DataContext = dc;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {

                dc.FileExtension = txtExt.Text;
                DialogResult = true;
                Close();

        }
    }
}
