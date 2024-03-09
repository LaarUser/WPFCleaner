//using WpfProjectCleanerMvvm;

namespace WpfProjectCleanerMvvmNet5
{
    public class Extension : ViewModelBase
    {
        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private bool _checked = false;
        public bool Checked { get => _checked; set { _checked = value; OnPropertyChanged(); } }

    }
}
