using WpfApp1.Dane;
using WpfApp1.Logika;

namespace WpfApp1.Prezentacja
{
    public class Model
    {
        private readonly LogikaApi _logika;

        public Model() : this(LogikaApi.TworzApi())
        {
            // jeśli ktoś wywoła konstruktor model, to wywoła się new Model(LogikaApi.TworzApi())
        }

        public Model(LogikaApi logika) // drugi konstruktor do testów
        {
            _logika = logika; // wstrzykiwanie zależności (DI)
        }

        public Task RozpocznijGreAsync(int kule)
        {
            return _logika.StartAsync(kule);
        }

        public void ZatrzymajGre()
        {
            _logika.Stop();
        }

        public IReadOnlyList<Kula> WezKule()
        {
            return _logika.PobierzWszystkieKule();
        }

        public event Action? Zmiana
        {
            add => _logika.PowiadomOZmianie += value; // gdy ktoś zapisze do Model.Zmiana, to przekaż to
            // do _logika.PowiadomOZmianie
            remove => _logika.PowiadomOZmianie -= value; // gdy ktoś wypisuje (usuwa) -> usuń go logiki
        }
    }
}
