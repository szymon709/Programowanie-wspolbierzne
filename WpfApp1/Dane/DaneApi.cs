using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;

namespace WpfApp1.Dane
{
    public static class Stol
    {
        public const double Szerokosc = 600;
        public const double Wysokosc = 300;
    }

    public class Kula
    {
        public int Id { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Srednica { get; private set; }
        public double Masa { get; private set; }
        public double PredkoscX { get; private set; }
        public double PredkoscY { get; private set; }

        public double Promien => Srednica / 2.0;
        public double SrodekX => X + Promien;
        public double SrodekY => Y + Promien;

        public Kula(int id, double x, double y, double srednica, double masa, double predkoscX, double predkoscY)
        {
            Id = id;
            X = x;
            Y = y;
            Srednica = srednica;
            Masa = masa;
            PredkoscX = predkoscX;
            PredkoscY = predkoscY;
        }

        public void Przesun(double deltaTime)
        {
            X += PredkoscX * deltaTime;
            Y += PredkoscY * deltaTime;
        }

        public void UstawPredkosc(double vx, double vy)
        {
            PredkoscX = vx;
            PredkoscY = vy;
        }

        public void UstawKule(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Kula Kopiuj()
        {
            return new Kula(
                Id,
                X,
                Y,
                Srednica,
                Masa,
                PredkoscX,
                PredkoscY
            );
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

    // logi
    internal class RejestratorDiagnostyczny : IDisposable
    {
        // kolejka wspólbieżna - nie ma bledow, jak wątki chcą logować jednocześnie
        private readonly ConcurrentQueue<string> _kolejkaLogow = new();
        private readonly CancellationTokenSource _zrodloAnulowania = new();
        private readonly Task _zadanieZapisu;

        public RejestratorDiagnostyczny()
        {
            _zadanieZapisu = Task.Run(() => ZapisujWtleAsync(_zrodloAnulowania.Token)); // zapis w tle
        }

        public void ZapiszStanKuli(Kula kula)
        {
            // serializacja do tekstu
            string json = JsonSerializer.Serialize(kula);
            string wpis = $"{DateTime.Now:O} | {json}";
            _kolejkaLogow.Enqueue(wpis);
        }

        private async Task ZapisujWtleAsync(CancellationToken token)
        {
            using var fileStream = new FileStream(
                "diagnostyka.log",
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete
            );

            using StreamWriter writer = new StreamWriter(fileStream, Encoding.ASCII)
            { 
                AutoFlush = true
            };

            while (!token.IsCancellationRequested || !_kolejkaLogow.IsEmpty)
            {
                if (_kolejkaLogow.TryDequeue(out string? wpis))
                {
                    await writer.WriteLineAsync(wpis);
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        }

        public void Dispose()
        {
            _zrodloAnulowania.Cancel();
            _zadanieZapisu.Wait();
            _zrodloAnulowania.Dispose();
        }
    }

    internal class DaneInstancja : DaneApi // internal -> widoczna w projekcie
    {
        private readonly List<Kula> _listKul = new(); // skrót "= new List<Kula>();"
        private readonly object _sekcjaKrytyczna = new(); // readonly -> nie można przypisać innej zmiennej po konstruktorze
        private readonly Random _losuj = new();
        private readonly RejestratorDiagnostyczny _rejestrator = new(); // logger

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

                    _listKul.Add(new Kula(
                        i,
                        pozycja.x,
                        pozycja.y,
                        srednica,
                        masa,
                        predkosc.vx,
                        predkosc.vy
                    ));
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

            List<Kula> kopiaPoAktualizacaji;

            lock (_sekcjaKrytyczna)
            {
                Parallel.ForEach(_listKul, kula =>
                {
                    kula.Przesun(deltaTime);
                });

                operacjePoRuchu?.Invoke(_listKul); // wykonaj to jeśli nie jest null

                kopiaPoAktualizacaji = _listKul.Select(k => k.Kopiuj()).ToList(); // kopia do logowania
            }
            foreach (var kula in kopiaPoAktualizacaji)
            {
                _rejestrator.ZapiszStanKuli(kula); // Zapis diagnostyczny po przesunięciu wszyskich 
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