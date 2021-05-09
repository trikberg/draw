using System;
using System.Collections.Generic;

namespace Draw.Shared.Draw
{
    public class CommandDrawLine : IDrawCommand
    {
        private List<DrawLineEventArgs> commands = new List<DrawLineEventArgs>();


        internal CommandDrawLine() { }

        internal bool IsClosed { get; private set; } = false;
        public IEnumerable<DrawLineEventArgs> DrawCommands => commands;

        internal void Add(DrawLineEventArgs args)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("Attemt to add line segment to closed CommandDrawLine.");
            }

            commands.Add(args);
            if (args.CloseLine)
            {
                IsClosed = true;
            }
        }

    }
}
