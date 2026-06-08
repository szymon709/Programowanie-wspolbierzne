using WpfApp1.Dane;
using WpfApp1.Logika;

namespace TestProject1
{
    internal class UdawaneDane : DaneApi
    {
        private readonly List<Kula> _kule = new();
        private readonly object _sekcjaKrytyczna = new();

        public UdawaneDane(IEnumerable<Kula>? kule = null)
        {
            if (kule != null)
            {
                _kule.AddRange(kule.Select(k => k.Kopiuj()));
            }
        }

        public override void StworzKule(int ileKul)
        {
            lock (_sekcjaKrytyczna)
            {
                _kule.Clear();

                for (int i = 0; i < ileKul; i++)
                {
                    _kule.Add(new Kula
                    (
                        i,
                        100 + i * 30,
                        100,
                        20,
                        1,
                        0,
                        0
                    ));
                }
            }
        }

        public override IReadOnlyList<Kula> PobierzKule()
        {
            lock (_sekcjaKrytyczna)
            {
                return _kule.Select(k => k.Kopiuj()).ToList();
            }
        }

        public override void AktualizujStan(double deltaTime, Action<IList<Kula>>? operacjePoRuchu = null)
        {
            lock (_sekcjaKrytyczna)
            {
                foreach (var kula in _kule)
                {
                    kula.Przesun(deltaTime);
                }

                operacjePoRuchu?.Invoke(_kule);
            }
        }
    }

    [TestClass]
    public class LogikaTests
    {
        [TestMethod]
        public void TestLogikaUzywaDaneApiPrzezDependencyInjection()
        {
            var fakeDane = new UdawaneDane(new[]
            {
                new Kula(1, 100, 100, 20, 1, 100, 0)
            });

            LogikaApi logika = LogikaApi.TworzApi(fakeDane);

            logika.WykonajKrok(0.5);

            var kula = logika.PobierzWszystkieKule().Single();
            Assert.AreEqual(150, kula.X, 0.000001);
        }

        [TestMethod]
        public void TestOdbiciaOdLewejSciany()
        {
            var fakeDane = new UdawaneDane(new[]
            {
                new Kula(1, -1, 100, 20, 1, -100, 0)
            });

            LogikaApi logika = LogikaApi.TworzApi(fakeDane);

            logika.WykonajKrok(0);

            var kula = logika.PobierzWszystkieKule().Single();
            Assert.AreEqual(0, kula.X, 0.000001);
            Assert.IsTrue(kula.PredkoscX > 0);
        }

        [TestMethod]
        public void TestZderzeniaDwochKulOTejSamejMasie()
        {
            var fakeDane = new UdawaneDane(new[]
            {
                new Kula(1, 100, 100, 20, 1, 10, 0),
                new Kula(2, 118, 100, 20, 1, -10, 0)
            });
        

            LogikaApi logika = LogikaApi.TworzApi(fakeDane);

            logika.WykonajKrok(0);

            var kule = logika.PobierzWszystkieKule().OrderBy(k => k.Id).ToList();

            Assert.AreEqual(-10, kule[0].PredkoscX, 0.000001);
            Assert.AreEqual(10, kule[1].PredkoscX, 0.000001);
        }

        [TestMethod]
        public void TestZderzeniaSprezystegoZachowujePedIEnergieKinetyczna()
        {
            var pierwsza = new Kula(1, 100, 100, 20, 2, 12, 0);
            var druga = new Kula(2, 118, 100, 20, 1, -6, 0);

            double pedPrzed = PoliczPedX(pierwsza, druga);
            double energiaPrzed = PoliczEnergieKinetyczna(pierwsza, druga);

            var fakeDane = new UdawaneDane(new[] { pierwsza, druga });
            LogikaApi logika = LogikaApi.TworzApi(fakeDane);

            logika.WykonajKrok(0);

            var kulePo = logika.PobierzWszystkieKule().OrderBy(k => k.Id).ToArray();
            double pedPo = PoliczPedX(kulePo[0], kulePo[1]);
            double energiaPo = PoliczEnergieKinetyczna(kulePo[0], kulePo[1]);

            Assert.AreEqual(pedPrzed, pedPo, 0.000001);
            Assert.AreEqual(energiaPrzed, energiaPo, 0.000001);
        }

        private static double PoliczPedX(params Kula[] kule)
        {
            return kule.Sum(k => k.Masa * k.PredkoscX);
        }

        private static double PoliczEnergieKinetyczna(params Kula[] kule)
        {
            return kule.Sum(k => 0.5 * k.Masa * (k.PredkoscX * k.PredkoscX + k.PredkoscY * k.PredkoscY));
        }
    }
}
