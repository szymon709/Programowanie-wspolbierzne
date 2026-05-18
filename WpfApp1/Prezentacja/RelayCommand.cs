using System.Windows.Input;

namespace WpfApp1.Prezentacja
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task>? _wykonajAsync; // funkcja bez param zwracająca Task
        private readonly Action? _wykonaj; // Action - funkcja void bez param
        private readonly Func<bool>? _czyMoznaWykonac; // funkcja bez param zwracająca bool
        private bool _czyTrwaWykonywanie;

        public RelayCommand(Action wykonaj, Func<bool>? czyMoznaWykonac = null)
        {
            _wykonaj = wykonaj;
            _czyMoznaWykonac = czyMoznaWykonac;
        }

        public RelayCommand(Func<Task> wykonajAsync, Func<bool>? czyMoznaWykonac = null)
        {
            _wykonajAsync = wykonajAsync;
            _czyMoznaWykonac = czyMoznaWykonac;
        }

        public bool CanExecute(object? parameter)
        { // "?." - jeżeli nie jest null
            return !_czyTrwaWykonywanie && (_czyMoznaWykonac?.Invoke() ?? true);
        } // "??" - jeśli wynik jest null -> zwróć "true"

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                _czyTrwaWykonywanie = true;
                RaiseCanExecuteChanged();

                if (_wykonajAsync is not null)
                {
                    await _wykonajAsync(); // uruchom async i nie blokuj UI
                }
                else
                {
                    _wykonaj?.Invoke();
                }
            }
            finally
            {
                _czyTrwaWykonywanie = false;
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
