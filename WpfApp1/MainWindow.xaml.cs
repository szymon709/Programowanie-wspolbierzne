using System.Windows;

namespace WpfApp1
{

    public partial class MainWindow : Window
    {
        private BoardManager _board;

        public MainWindow()
        {
            InitializeComponent();

            _board = new BoardManager();
            _board.AddBall(new Ball(1, 0.0, 0.0));
            _board.AddBall(new Ball(2, 1.0, 1.5));
            _board.AddBall(new Ball(3, 5.0, 2.5));

            _board.MoveBalls();
            BallsList.ItemsSource = _board.GetBalls();
        }
    }
}