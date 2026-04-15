using System;
using System.Collections.Generic;
using System.Threading;
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

        public LogikaInstancja(DaneApi dane)
        {
            _dane = dane;
        }

        public override void Start(int liczbaKul)
        {
            _dane.StworzKule(liczbaKul);
            _stoper = new Timer(Ruch, null, 0, 20);   // co 20 ms wykonaj ruch
        }

        private void Ruch(object stan)
        {
            foreach (var kula in _dane.PobierzKule())
            {
                // zmiana pozycji o ustalona predkosc
                kula.X += kula.PredkoscX;
                kula.Y += kula.PredkoscY;

                // odbijanie od scian po X
                if (kula.X <= 0 || kula.X + kula.Srednica >= Stol.Szerokosc)
                {
                    kula.PredkoscX = -kula.PredkoscX; // zmiana kierunku na przeciwny
                }

                // odbjinanie od scian po Y
                if (kula.Y <= 0 || kula.Y + kula.Srednica >= Stol.Wysokosc)
                {
                    kula.PredkoscY = -kula.PredkoscY;
                }
            }
            PowiadomOZmianie?.Invoke();
        }

        public override List<Kula> PobierzWszystkieKule() => _dane.PobierzKule();
    }
}