namespace Draw.Shared.Draw
{
    public class CommandBackground : IDrawCommand
    {
        public CommandBackground(string backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public string BackgroundColor { get; private set; }
    }
}
