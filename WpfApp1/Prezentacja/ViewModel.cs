using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Dane;

namespace WpfApp1.Prezentacja
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Model _model = new Model();
        private int _liczbaKul;

        public ObservableCollection<Kula> Kule { get; } = new ObservableCollection<Kula>();
        public ICommand PrzyciskStart { get; }

        public int LiczbaKul
        {
            get => _liczbaKul;
            set
            {
                _liczbaKul = value;
                OnPropertyChanged("LiczbaKul");
            }
        }

        public ViewModel()
        {
            PrzyciskStart = new RelayCommand(() => _model.RozpocznijGre(LiczbaKul));
            _model.Zmiana += OdswiezKuleNaEkranie;
        }

        private void OdswiezKuleNaEkranie()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var kuleZModelu = _model.WezKule();

                Kule.Clear();
                foreach (var k in kuleZModelu)
                {
                    Kule.Add(k);
                }
                
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nazwa) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nazwa));
    }
}