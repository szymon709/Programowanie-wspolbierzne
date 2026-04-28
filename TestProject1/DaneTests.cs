using WpfApp1.Dane;

namespace TestProject1
{
    [TestClass]
    public class DaneTests
    {
        [TestMethod]
        public void TestTworzeniaKul()
        {
            DaneApi api = DaneApi.TworzApi();
            int oczekiwanaLiczbaKul = 10;

            api.StworzKule(oczekiwanaLiczbaKul);

            Assert.HasCount(oczekiwanaLiczbaKul, api.PobierzKule());

            foreach (var kula in api.PobierzKule())
            {
                Assert.IsNotNull(kula);
                Assert.IsTrue(kula.X >= 0 && kula.X <= Stol.Szerokosc - kula.Srednica);
                Assert.IsTrue(kula.Y >= 0 && kula.Y <= Stol.Wysokosc - kula.Srednica);
            }
        }
    }
}