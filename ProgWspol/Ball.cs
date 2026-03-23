namespace ballnamespace
{
    public class Ball
    {

        private int Id;
        private double X;
        private double Y;

        public Ball(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public double GetX()
        {
            return X;
        }

        public double GetY()
        {
            return Y;
        }

        public int GetId()
        {
            return Id;
        }

        public void SetX(double x)
        {
            X = x;
        }

        public void SetY(double y)
        {
            Y = y;
        }
    }
}