using System.Diagnostics;
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
        private const double DeltaTime = InterwalMs / 1000.0;

        private readonly DaneApi _dane;
        private readonly object _sekcjaStartStop = new();
        private CancellationTokenSource? _zrodloAnulowania;
        private Task? _zadanieSymulacji;

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

            Stop(); // zatrzymaj poprzednie symulacje
            _dane.StworzKule(liczbaKul);
            PowiadomOZmianie?.Invoke(); // "?." jeśli nie jest null wykonaj Invoke() w tym przypadku odświeżenie ekranu

            var noweZrodlo = new CancellationTokenSource(); // obiekt do zatrzymywania pętli async

            lock (_sekcjaStartStop)
            {
                _zrodloAnulowania = noweZrodlo; // przypisujemy ten obiekt
                _zadanieSymulacji = Task.Run(() => PetlaSymulacjiAsync(noweZrodlo.Token));
            } // linia wyżej uruchamia taska w tle

            return Task.CompletedTask;
        }

        public override void Stop()
        {
            lock (_sekcjaStartStop)
            {
                _zrodloAnulowania?.Cancel();
                _zrodloAnulowania?.Dispose();
                _zrodloAnulowania = null;
                _zadanieSymulacji = null;
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

        private async Task PetlaSymulacjiAsync(CancellationToken token)
        {
            // programowanie czasu rzeczywistego, liczymy rzeczywisty upływ czasu
            Stopwatch stoper = Stopwatch.StartNew();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    double rzeczywistyCzas = stoper.Elapsed.TotalSeconds;
                    stoper.Restart();

                    WykonajKrok(rzeczywistyCzas);

                    await Task.Delay(InterwalMs, token);
                }
            }
            catch (OperationCanceledException)
            {
                // zatrzymanie symulacji
            }
        }

        private static void ObsluzKolizjeZeScianami(IList<Kula> kule)
        {
            foreach (var kula in kule)
            {
                if (kula.X < 0)
                {
                    kula.X = 0;
                    kula.PredkoscX = Math.Abs(kula.PredkoscX);
                }
                else if (kula.X + kula.Srednica > Stol.Szerokosc)
                {
                    kula.X = Stol.Szerokosc - kula.Srednica;
                    kula.PredkoscX = -Math.Abs(kula.PredkoscX);
                }

                if (kula.Y < 0)
                {
                    kula.Y = 0;
                    kula.PredkoscY = Math.Abs(kula.PredkoscY);
                }
                else if (kula.Y + kula.Srednica > Stol.Wysokosc)
                {
                    kula.Y = Stol.Wysokosc - kula.Srednica;
                    kula.PredkoscY = -Math.Abs(kula.PredkoscY);
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

            pierwsza.PredkoscX -= ped * odwrotnoscMasyPierwszej * nx;
            pierwsza.PredkoscY -= ped * odwrotnoscMasyPierwszej * ny;

            druga.PredkoscX += ped * odwrotnoscMasyDrugiej * nx;
            druga.PredkoscY += ped * odwrotnoscMasyDrugiej * ny;
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

            pierwsza.X -= nx * przesunieciePierwszej;
            pierwsza.Y -= ny * przesunieciePierwszej;
            druga.X += nx * przesuniecieDrugiej;
            druga.Y += ny * przesuniecieDrugiej;
        }
    }
}