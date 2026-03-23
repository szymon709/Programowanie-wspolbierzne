namespace ballnamespace
{
    public class BoardManager
    {
        private Random random = new Random();
        private List<Ball> _balls = new List<Ball>();
        private double _boardWidth = 100.0;
        private double _boardHeight = 100.0;

        public void AddBall(Ball ball)
        {
            _balls.Add(ball);
        }

        public List<Ball> GetBalls()
        {
            return _balls;
        }

        public void MoveBalls()
        {

            foreach (var ball in _balls)
            {
                double newX = Math.Round(ball.GetX() + random.NextDouble(), 1);
                double newY = Math.Round(ball.GetY() + random.NextDouble(), 1);
                ball.SetX(newX);
                ball.SetY(newY);
            }
        }

    }
}