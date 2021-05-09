using System;

namespace Draw.Shared.Draw
{
    public class DrawLineEventArgs : IDrawEventArgs
    {
        public DrawLineEventArgs(Point2D p1, Point2D p2, int brushSize, string color, bool closeLine)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.BrushSize = brushSize;
            this.Color = color;
            this.CloseLine = closeLine;
        }

        public Point2D P1 { get; set; }
        public Point2D P2 { get; set; }
        public int BrushSize { get; set; }
        public string Color { get; set; }

        public bool CloseLine { get; set; }
    }
}