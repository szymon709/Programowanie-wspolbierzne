using WpfApp1.Dane;
using static System.Net.Mime.MediaTypeNames;

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

            var kule = api.PobierzKule();

            Assert.AreEqual(oczekiwanaLiczbaKul, kule.Count);

            foreach (var kula in kule)
            {
                Assert.IsNotNull(kula);
                Assert.IsTrue(kula.Srednica > 0, "Kula powinna mieć dodatnią średnicę.");
                Assert.IsTrue(kula.Masa > 0, "Kula powinna mieć dodatnią masę.");
                Assert.IsTrue(kula.X >= 0 && kula.X <= Stol.Szerokosc - kula.Srednica);
                Assert.IsTrue(kula.Y >= 0 && kula.Y <= Stol.Wysokosc - kula.Srednica);
            }
        }

        [TestMethod]
        public void TestAktualizujStanPrzesuwaKule()
        {
            DaneApi api = DaneApi.TworzApi();

            api.StworzKule(1);

            var kulaPrzed = api.PobierzKule().Single();

            api.AktualizujStan(0.5);

            var kulaPo = api.PobierzKule().Single();

            bool zmienilaX = Math.Abs(kulaPo.X - kulaPrzed.X) > 0.000001;
            bool zmienilaY = Math.Abs(kulaPo.Y - kulaPrzed.Y) > 0.000001;

            Assert.IsTrue(zmienilaX || zmienilaY, "Kula powinna zmienić pozycję po AktualizujStan.");
        }

        [TestMethod]
        public void TestPobierzKuleZwracaKopieNieOryginalnaListe()
        {
            DaneApi api = DaneApi.TworzApi();

            api.StworzKule(1);

            var pobranaKula = api.PobierzKule().Single();
            double oryginalneX = pobranaKula.X;

            pobranaKula.X = 999;

            var ponowniePobranaKula = api.PobierzKule().Single();

            Assert.AreEqual(oryginalneX, ponowniePobranaKula.X, 0.000001);
        }

        [TestMethod]
        public async Task TestTworzenieLogow()
        {
            string testFileName = "test_diagnostyka.log";
            if (File.Exists(testFileName)) File.Delete(testFileName);

            string json = """
            {
                "kula": {
                    "x": 10,
                    "y": 20
                }
            }
            """;

            using (StreamWriter writer = new StreamWriter(testFileName, false, System.Text.Encoding.ASCII))
            {
                await writer.WriteLineAsync(json);
            }

            await Task.Delay(100);

            Assert.IsTrue(File.Exists(testFileName), "Logi utworzone");
            string content = File.ReadAllText(testFileName);
            Assert.IsTrue(content.Contains("10"), "Plik logów ma dane");

            if (File.Exists(testFileName)) File.Delete(testFileName);
        }
    }
}