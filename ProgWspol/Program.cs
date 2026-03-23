namespace ballnamespace
{
    public class Program
    {
        static void Main(string[] args)
        {
            BoardManager board = new BoardManager();

            board.AddBall(new Ball(1, 0.0, 0.0));
            board.AddBall(new Ball(2, 1.0, 1.5));
            board.AddBall(new Ball(3, 5.0, 2.5));

            List<Ball> balls = board.GetBalls();



            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("iteracja: " + i);
                foreach (var ball in balls)
                {
                    Console.WriteLine($"Piłka: {ball.GetId()}, x: {ball.GetX()}, y: {ball.GetY()}.");
                }
                board.MoveBalls();
                Console.WriteLine();
            }
        }
    }
}