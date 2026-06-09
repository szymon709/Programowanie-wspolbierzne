using WpfApp1.Dane;

namespace WpfApp1.Logika
{
    public abstract class LogikaApi
    {
        public abstract Task StartAsync(int liczbaKul); // Task - operacja asynchroniczna
        public abstract void Stop();
        public abstract void WykonajKrok(double deltaTime);
        public abstract IReadOnlyList<Kula> PobierzWszystkieKule();
        public abstract event Action? PowiadomOZmianie; // "?" - to może być null

        public static LogikaApi TworzApi(DaneApi? dane = null)
        {
            return new LogikaInstancja(dane ?? DaneApi.TworzApi()); // wstrzykiwanie zależności
        } // "??" jeśli lewa stron jest null to użyj prawej
    }

    internal class LogikaInstancja : LogikaApi
    {
        private const int InterwalMs = 20;

        private readonly DaneApi _dane;
        private readonly object _sekcjaStartStop = new();

        private System.Timers.Timer? _timerSymulacji;
        private DateTime _ostatniCzas;
        private int _czyKrokWToku; // flaga zabezpieczajaca przed nakładaniem się kroków symulacji

        public override event Action? PowiadomOZmianie;

        public LogikaInstancja(DaneApi dane)
        {
            _dane = dane;
        }

        public override Task StartAsync(int liczbaKul)
        {
            if (liczbaKul <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(liczbaKul), "Liczba kul musi być większa od zera.");
            }

            Stop();

            _dane.StworzKule(liczbaKul);
            PowiadomOZmianie?.Invoke();

            lock (_sekcjaStartStop)
            {
                _ostatniCzas = DateTime.UtcNow;
                _czyKrokWToku = 0; // 0 - nie w toku, 1 - w toku

                _timerSymulacji = new System.Timers.Timer(InterwalMs);
                _timerSymulacji.AutoReset = true;
                _timerSymulacji.Elapsed += TimerSymulacjiElapsed;
                _timerSymulacji.Start();
            }

            return Task.CompletedTask;
        }

        public override void Stop()
        {
            lock (_sekcjaStartStop)
            {
                if (_timerSymulacji is not null)
                {
                    _timerSymulacji.Stop();
                    _timerSymulacji.Elapsed -= TimerSymulacjiElapsed;
                    _timerSymulacji.Dispose();
                    _timerSymulacji = null;
                }
            }
        }

        private void TimerSymulacjiElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            // zabezpieczenie flaga by nie wywolalo sie wiecej razy niz raz w tym samym czasie
            if (Interlocked.Exchange(ref _czyKrokWToku, 1) == 1)
            {
                return;
            }

            try
            {
                DateTime teraz = DateTime.UtcNow;

                double deltaTime = (teraz - _ostatniCzas).TotalSeconds;
                _ostatniCzas = teraz;

                if (deltaTime < 0)
                {
                    return;
                }



                WykonajKrok(deltaTime);
            }
            finally
            {
                Interlocked.Exchange(ref _czyKrokWToku, 0);
            }
        }


        public override void WykonajKrok(double deltaTime)
        {
            _dane.AktualizujStan(deltaTime, kule =>
            {
                ObsluzKolizjeZeScianami(kule);
                ObsluzKolizjeMiedzyKulami(kule);
            });

            PowiadomOZmianie?.Invoke();
        }

        public override IReadOnlyList<Kula> PobierzWszystkieKule()
        {
            return _dane.PobierzKule();
        }

        private static void ObsluzKolizjeZeScianami(IList<Kula> kule)
        {
            foreach (var kula in kule)
            {
                if (kula.X < 0)
                {
                    kula.UstawKule(0, kula.Y);
                    kula.UstawPredkosc(Math.Abs(kula.PredkoscX), kula.PredkoscY);
                }
                else if (kula.X + kula.Srednica > Stol.Szerokosc)
                {
                    kula.UstawKule(Stol.Szerokosc - kula.Srednica, kula.Y);
                    kula.UstawPredkosc(-Math.Abs(kula.PredkoscX), kula.PredkoscY);
                }

                if (kula.Y < 0)
                {
                    kula.UstawKule(kula.X, 0);
                    kula.UstawPredkosc(kula.PredkoscX, Math.Abs(kula.PredkoscY));
                }
                else if (kula.Y + kula.Srednica > Stol.Wysokosc)
                {
                    kula.UstawKule(kula.X, Stol.Wysokosc - kula.Srednica);
                    kula.UstawPredkosc(kula.PredkoscX, -Math.Abs(kula.PredkoscY));
                }
            }
        }

        private static void ObsluzKolizjeMiedzyKulami(IList<Kula> kule)
        {
            for (int i = 0; i < kule.Count; i++)
            {
                for (int j = i + 1; j < kule.Count; j++)
                {
                    ObsluzKolizjeKul(kule[i], kule[j]);
                }
            }
        }

        private static void ObsluzKolizjeKul(Kula pierwsza, Kula druga)
        {
            double dx = druga.SrodekX - pierwsza.SrodekX;
            double dy = druga.SrodekY - pierwsza.SrodekY;
            double minimalnaOdleglosc = pierwsza.Promien + druga.Promien;
            double odlegloscKwadrat = dx * dx + dy * dy;

            if (odlegloscKwadrat > minimalnaOdleglosc * minimalnaOdleglosc)
            {
                return;
            }

            double odleglosc = Math.Sqrt(odlegloscKwadrat);

            if (odleglosc < 0.000001) // gdy kule wejdą w siebie (zabezpieczenie)
            { // rozsunięcie
                dx = 1;
                dy = 0;
                odleglosc = 1;
            }

            // normalna zderzenia - kierunek
            double nx = dx / odleglosc;
            double ny = dy / odleglosc;

            RozsunKule(pierwsza, druga, minimalnaOdleglosc - odleglosc, nx, ny);

            // prędkość
            double predkoscWzglednaX = druga.PredkoscX - pierwsza.PredkoscX;
            double predkoscWzglednaY = druga.PredkoscY - pierwsza.PredkoscY;
            double predkoscWzglednaWNormalnej = predkoscWzglednaX * nx + predkoscWzglednaY * ny;

            // jeżeli kule już się od siebie oddalają to pomiń
            if (predkoscWzglednaWNormalnej >= 0)
            {
                return;
            }

            const double wspolczynnikSprezystosci = 1.0; // odbicie sprężyste
            double odwrotnoscMasyPierwszej = 1.0 / pierwsza.Masa;
            double odwrotnoscMasyDrugiej = 1.0 / druga.Masa;

            double ped = -(1.0 + wspolczynnikSprezystosci) * predkoscWzglednaWNormalnej;
            ped /= odwrotnoscMasyPierwszej + odwrotnoscMasyDrugiej;

            pierwsza.UstawPredkosc(
                pierwsza.PredkoscX - ped * odwrotnoscMasyPierwszej * nx,
                pierwsza.PredkoscY - ped * odwrotnoscMasyPierwszej * ny
            );

            druga.UstawPredkosc(
                druga.PredkoscX + ped * odwrotnoscMasyDrugiej * nx,
                druga.PredkoscY + ped * odwrotnoscMasyDrugiej * ny
            );
        }

        private static void RozsunKule(Kula pierwsza, Kula druga, double nalozenie, double nx, double ny)
        {
            if (nalozenie <= 0)
            {
                return;
            }

            double odwrotnoscMasyPierwszej = 1.0 / pierwsza.Masa;
            double odwrotnoscMasyDrugiej = 1.0 / druga.Masa;
            double sumaOdwrotnosciMas = odwrotnoscMasyPierwszej + odwrotnoscMasyDrugiej;

            double przesunieciePierwszej = nalozenie * (odwrotnoscMasyPierwszej / sumaOdwrotnosciMas);
            double przesuniecieDrugiej = nalozenie * (odwrotnoscMasyDrugiej / sumaOdwrotnosciMas);

            pierwsza.UstawKule(
                pierwsza.X - nx * przesunieciePierwszej,
                pierwsza.Y - ny * przesunieciePierwszej
            );

            druga.UstawKule(
                druga.X + nx * przesuniecieDrugiej,
                druga.Y + ny * przesuniecieDrugiej
            );
        }
    }
}