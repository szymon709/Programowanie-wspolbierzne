using WpfApp1.Dane;

namespace WpfApp1.Logika
{
    public abstract class LogikaApi
    {
        public abstract void Start(int liczbaKul);
        public abstract List<Kula> PobierzWszystkieKule();
        public abstract event Action PowiadomOZmianie;

        public static LogikaApi TworzApi(DaneApi dane = null)
        {
            return new LogikaInstancja(dane ?? DaneApi.TworzApi());
        }
    }
    
    internal class LogikaInstancja : LogikaApi
    {
        private readonly DaneApi _dane;
        private Timer _stoper;
        public override event Action PowiadomOZmianie;
        private const int InterwalMs = 20;

        public LogikaInstancja(DaneApi dane)
        {
            _dane = dane;
        }

        public override void Start(int liczbaKul)
        {
            _stoper?.Dispose();
            _dane.StworzKule(liczbaKul);
            _stoper = new Timer(Ruch, null, 0, InterwalMs);
        }

        public void Stop()
        {
            _stoper?.Dispose();
            _stoper = null;
        }

        private void Ruch(object stan)
        {
            foreach (var kula in _dane.PobierzKule())
            {
                kula.X += kula.PredkoscX;
                kula.Y += kula.PredkoscY;

                // odbijanie od scian
                if (kula.X <= 0 || kula.X + kula.Srednica >= Stol.Szerokosc)
                {
                    kula.PredkoscX = -kula.PredkoscX;
                }

                if (kula.Y <= 0 || kula.Y + kula.Srednica >= Stol.Wysokosc)
                {
                    kula.PredkoscY = -kula.PredkoscY;
                }
            }
            PowiadomOZmianie?.Invoke();
        }

        public override List<Kula> PobierzWszystkieKule() => new List<Kula>(_dane.PobierzKule());
    }
}