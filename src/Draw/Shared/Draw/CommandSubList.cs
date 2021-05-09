using System.Linq;
using System.Collections.Generic;
using System;

namespace Draw.Shared.Draw
{
    internal class CommandSubList
    {
        private List<IDrawCommand> commands = new List<IDrawCommand>();

        public CommandSubList(string backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        internal int Count => commands.Count;
        internal string BackgroundColor { get; private set; }

        internal void Add(IDrawCommand command)
        {
            commands.Add(command);
        }

        internal List<IDrawCommand> ToList()
        {
            return new List<IDrawCommand>(commands);
        }

        internal void RemoveLast()
        {
            if (commands.Count > 0)
            {
                commands.RemoveAt(commands.Count - 1);
            }
        }

        internal IDrawCommand Last()
        {
            return commands.LastOrDefault();
        }
    }
}
