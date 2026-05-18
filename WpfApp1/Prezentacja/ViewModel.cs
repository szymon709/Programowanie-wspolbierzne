using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Dane;

namespace WpfApp1.Prezentacja
{
    public class ViewModel : INotifyPropertyChanged // komunikacja między UI a programem
    {
        private readonly Model _model;
        private int _liczbaKul = 10;
        private string _komunikat = "Gotowy";

        public ObservableCollection<Kula> Kule { get; } = new();
        public ICommand PrzyciskStart { get; }
        public ICommand PrzyciskStop { get; }

        public int LiczbaKul
        {
            get => _liczbaKul; // return
            set
            {
                if (_liczbaKul == value) return; // value to specjalna zmienna
                _liczbaKul = Math.Max(0, value);
                OnPropertyChanged();

                if (PrzyciskStart is RelayCommand komenda) // jeżeli PrzyciskS to RelayC to zapisz jako komenda
                {
                    komenda.RaiseCanExecuteChanged(); // może być wykonane (odświeżenie)
                }
            }
        }

        public string Komunikat
        {
            get => _komunikat;
            set
            {
                if (_komunikat == value) return;
                _komunikat = value;
                OnPropertyChanged();
            }
        }

        public ViewModel() : this(new Model())
        {
        }

        public ViewModel(Model model)
        {
            _model = model;
            PrzyciskStart = new RelayCommand(RozpocznijGreAsync, () => LiczbaKul > 0); // pozwól wykonać RozpocznijGA, gdy LiczbaK > 0
            PrzyciskStop = new RelayCommand(ZatrzymajGre); // "() => LiczbaKul > 0" - to lambda, zwraca True/False
            _model.Zmiana += OdswiezKuleNaEkranie; // "=>" lewa strona argumenty, prawa wynik
        } // linia wyżej - gdy Logika powie zmiana to uruchamiane jest OdswiezKuleNaEkranie

        private async Task RozpocznijGreAsync()
        {
            try
            {
                await _model.RozpocznijGreAsync(LiczbaKul);
                Komunikat = $"Symulacja uruchomiona. Liczba kul: {LiczbaKul}";
            }
            catch (Exception ex)
            {
                Komunikat = $"Błąd: {ex.Message}";
            }
        }

        private void ZatrzymajGre()
        {
            _model.ZatrzymajGre();
            Komunikat = "Symulacja zatrzymana";
        }

        private void OdswiezKuleNaEkranie()
        {
            var kuleZModelu = _model.WezKule();

            Application.Current.Dispatcher.BeginInvoke(() => // BeginInvoke - zadanie jest kolejkowane i nie musi czekać aż zostanie wykonane
            {
                Kule.Clear();
                foreach (var k in kuleZModelu)
                {
                    Kule.Add(k);
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged; // mechanizm WPF - odświeża "Binding" w UI

        protected void OnPropertyChanged([CallerMemberName] string? nazwa = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nazwa));
        }
    }
}
