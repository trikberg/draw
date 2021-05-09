namespace Draw.Shared.Draw
{
    public class FillEventArgs : IDrawEventArgs
    {
        public FillEventArgs(Point2D point, string color)
        {
            this.Point = point;
            this.Color = color;
        }

        public Point2D Point { get; set; }
        public string Color { get; set; }
    }
}