using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draw.Shared.Draw
{
    public class Point2D
    {
        public Point2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public double Distance(Point2D mousePoint)
        {
            if (mousePoint == null)
            {
                throw new ArgumentNullException();
            }

            return Math.Sqrt(Math.Pow((mousePoint.X - X), 2) + Math.Pow((mousePoint.Y - Y), 2));
        }
    }
}
