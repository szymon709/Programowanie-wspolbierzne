namespace WpfApp1.Dane
{
    public static class Stol
    {
        public const double Szerokosc = 600;
        public const double Wysokosc = 300;
    }

    public class Kula
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Srednica { get; set; }
        public double Masa { get; set; }
        public double PredkoscX { get; set; }
        public double PredkoscY { get; set; }

        public double Promien => Srednica / 2.0;
        public double SrodekX => X + Promien;
        public double SrodekY => Y + Promien;

        public void Przesun(double deltaTime)
        {
            X += PredkoscX * deltaTime;
            Y += PredkoscY * deltaTime;
        }

        public Kula Kopiuj()
        {
            return new Kula
            {
                Id = Id,
                X = X,
                Y = Y,
                Srednica = Srednica,
                Masa = Masa,
                PredkoscX = PredkoscX,
                PredkoscY = PredkoscY
            };
        }
    }


    public abstract class DaneApi
    {
        public abstract void StworzKule(int ileKul);
        public abstract IReadOnlyList<Kula> PobierzKule(); // tylko do odczytu

        public abstract void AktualizujStan(double deltaTime, Action<IList<Kula>>? operacjePoRuchu = null);

        public static DaneApi TworzApi()
        {
            return new DaneInstancja();
        }
    }

    internal class DaneInstancja : DaneApi // internal -> widoczna w projekcie
    {
        private readonly List<Kula> _listKul = new(); // skrót "= new List<Kula>();"
        private readonly object _sekcjaKrytyczna = new(); // readonly -> nie można przypisać innej zmiennej po konstruktorze
        private readonly Random _losuj = new(); // ale zawartość można

        public override void StworzKule(int ileKul)
        {
            if (ileKul < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ileKul), "Liczba kul nie może być ujemna.");
            }

            lock (_sekcjaKrytyczna) // zamek albo wątek symulacji aktualizuje kule
            {                       // albo UI odświeża kule
                _listKul.Clear();

                const double srednica = 20;
                double masa = ObliczMase(srednica);

                for (int i = 0; i < ileKul; i++)
                {
                    var pozycja = WylosujBezpiecznaPozycje(srednica); // "var" - kompilator sam wybiera typ
                    var predkosc = WylosujPredkosc();

                    _listKul.Add(new Kula
                    {
                        Id = i,
                        Srednica = srednica,
                        Masa = masa,
                        X = pozycja.x,
                        Y = pozycja.y,
                        PredkoscX = predkosc.vx,
                        PredkoscY = predkosc.vy
                    });
                }
            }
        }

        public override IReadOnlyList<Kula> PobierzKule()
        {
            lock (_sekcjaKrytyczna)
            {
                return _listKul.Select(k => k.Kopiuj()).ToList();
            }
        }

        public override void AktualizujStan(double deltaTime, Action<IList<Kula>>? operacjePoRuchu = null)
        {
            if (deltaTime < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta czasu nie może być ujemna.");
            }

            lock (_sekcjaKrytyczna)
            {
                foreach (var kula in _listKul)
                {
                    kula.Przesun(deltaTime);
                }

                operacjePoRuchu?.Invoke(_listKul); // wykonaj to jeśli nie jest null
            }
        }


        private static double ObliczMase(double srednica)
        {
            double promien = srednica / 2.0;
            return Math.PI * promien * promien;
        }

        private (double x, double y) WylosujBezpiecznaPozycje(double srednica)
        {
            const int maksymalnaLiczbaProb = 1000;
            double promien = srednica / 2.0;

            for (int proba = 0; proba < maksymalnaLiczbaProb; proba++)
            {
                double x = _losuj.NextDouble() * (Stol.Szerokosc - srednica);
                double y = _losuj.NextDouble() * (Stol.Wysokosc - srednica);

                bool kolizjaNaStarcie = _listKul.Any(k => // Any - czy istnieje jaki kolwiek (ma to sens Any)
                {
                    double dx = (x + promien) - k.SrodekX;
                    double dy = (y + promien) - k.SrodekY;
                    double minimalnaOdleglosc = promien + k.Promien;
                    return dx * dx + dy * dy < minimalnaOdleglosc * minimalnaOdleglosc;
                });

                if (!kolizjaNaStarcie)
                {
                    return (x, y);
                }
            }

            // gdy za dużo kul
            return (
                _losuj.NextDouble() * (Stol.Szerokosc - srednica),
                _losuj.NextDouble() * (Stol.Wysokosc - srednica)
            );
        }

        private (double vx, double vy) WylosujPredkosc()
        {
            double kat = _losuj.NextDouble() * 2.0 * Math.PI;
            double szybkosc = 80.0 + _losuj.NextDouble() * 80.0; // piksele na sekundę

            return (Math.Cos(kat) * szybkosc, Math.Sin(kat) * szybkosc);
        }
    }
}
