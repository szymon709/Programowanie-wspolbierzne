namespace TestProject;
using ballnamespace;

    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
        BoardManager board = new BoardManager();
        double x = 0.0;
        double y = 0.0;
        board.AddBall(new Ball(1, x, y));
        board.MoveBalls();
        List<Ball> balls = board.GetBalls();
        Ball ball1 = balls[0];
        Assert.AreNotEqual(x, ball1.GetX());
        }
    }