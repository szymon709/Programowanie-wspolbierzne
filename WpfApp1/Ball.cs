using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp1
{
    public class Ball
    {

        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

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
