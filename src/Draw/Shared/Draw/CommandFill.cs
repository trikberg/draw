
namespace Draw.Shared.Draw
{
    public class CommandFill : IDrawCommand
    {
        internal CommandFill(FillEventArgs args) 
        {
            FillArgs = args;
        }

        public FillEventArgs FillArgs { get; }
    }
}
