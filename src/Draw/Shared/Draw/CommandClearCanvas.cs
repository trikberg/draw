namespace Draw.Shared.Draw
{
    public class CommandClearCanvas : IDrawCommand
    {
        public CommandClearCanvas(string backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public string BackgroundColor { get; private set; }
    }
}
