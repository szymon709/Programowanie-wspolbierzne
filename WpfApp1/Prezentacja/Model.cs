using WpfApp1.Logika;
using WpfApp1.Dane;

namespace WpfApp1.Prezentacja
{
    public class Model
    {
        private LogikaApi _logika;

        public Model()
        {
            _logika = LogikaApi.TworzApi();
        }

        public void RozpocznijGre(int kule) => _logika.Start(kule);
        public List<Kula> WezKule() => _logika.PobierzWszystkieKule();

        public event Action Zmiana
        {
            add => _logika.PowiadomOZmianie += value;
            remove => _logika.PowiadomOZmianie -= value;
        }
    }
}