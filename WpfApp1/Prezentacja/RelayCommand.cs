using System.Windows.Input;

namespace WpfApp1.Prezentacja
{
    public class RelayCommand : ICommand
    {
        private readonly Action _wykonaj;
        public RelayCommand(Action wykonaj) => _wykonaj = wykonaj;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _wykonaj();
        public event EventHandler CanExecuteChanged;
    }
}