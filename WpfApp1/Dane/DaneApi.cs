using System.ComponentModel;

namespace WpfApp1.Dane
{
    public class Stol
    {
        public static int Szerokosc = 600; 
        public static int Wysokosc = 300;
    }

    public class Kula : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        public int Id { get; set; }
        public double X {
            get => _x;
            set { _x = value; OnPropertyChanged(nameof(X)); }
        }
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(nameof(Y)); }
        }
        public double Srednica { get; set; }

        public double PredkoscX { get; set; }
        public double PredkoscY { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public abstract class DaneApi
    {
        public abstract void StworzKule(int ileKul);
        public abstract List<Kula> PobierzKule();

        public static DaneApi TworzApi()
        {
            return new DaneInstancja();
        }
    }

    internal class DaneInstancja : DaneApi
    {
        private readonly List<Kula> _listKul = new List<Kula>();

        public override void StworzKule(int ileKul)
        {
            _listKul.Clear();
            Random losuj = new Random();

            int SrednicaDoLosowania = 20;

            for (int i = 0; i < ileKul; i++)
            {
                _listKul.Add(new Kula
                {
                    Srednica = SrednicaDoLosowania,
                    Id = i,
                    X = losuj.Next(SrednicaDoLosowania, Stol.Szerokosc - SrednicaDoLosowania),
                    Y = losuj.Next(SrednicaDoLosowania, Stol.Wysokosc - SrednicaDoLosowania),

                    PredkoscX = losuj.NextDouble() * 4 - 2,   // nextdouble losuje 0.0 - 1.0
                    PredkoscY = losuj.NextDouble() * 4 - 2
                });
            }
        }

        public override List<Kula> PobierzKule() => _listKul;
    }
}