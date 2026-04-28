using WpfApp1.Dane;
using WpfApp1.Logika;

namespace TestProject1
{
    internal class UdawaneDane : DaneApi
    {
        public List<Kula> Kule = new List<Kula>();
        public override void StworzKule(int ile) { }
        public override List<Kula> PobierzKule() => Kule;
    }

    [TestClass]
    public class LogikaTests
    {
        [TestMethod]
        public void TestRuchuKul()
        {
            var fakeDane = new UdawaneDane();
            fakeDane.Kule.Add(new Kula { X = 100, Y = 100, PredkoscX = 5, PredkoscY = 5, Srednica = 20 });

            LogikaApi logika = LogikaApi.TworzApi(fakeDane);

            var pobraneKule = logika.PobierzWszystkieKule();

            Assert.HasCount(1, pobraneKule);
            Assert.AreEqual(100, pobraneKule[0].X);
        }
    }
}