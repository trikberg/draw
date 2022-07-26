using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Draw.Shared.Draw
{
    public class CommandList : IEnumerable<IDrawCommand>
    {
        private List<CommandSubList> stack = new List<CommandSubList>();

        public CommandList(string backgroundColor)
        {
            stack.Add(new CommandSubList(backgroundColor));
        }

        public void Clear(string backgroundColor)
        {
            stack.Clear();
            stack.Add(new CommandSubList(backgroundColor));
        }

        public void Add(IDrawCommand command)
        {
            lock (stack)
            {
                CommandSubList? currentList;
                if (command is CommandClearCanvas ccc)
                {
                    currentList = new CommandSubList(ccc.BackgroundColor);
                    stack.Add(currentList);
                }
                else
                {
                    currentList = stack.LastOrDefault();
                    currentList?.Add(command);
                }
            }
        }

        public void Add(DrawLineEventArgs lineArgs)
        {
            lock (stack)
            {
                CommandSubList? currentList = stack.LastOrDefault();
                if (currentList != null)
                {
                    if (currentList.Last() is CommandDrawLine cdl &&
                        !cdl.IsClosed)
                    {
                        cdl.Add(lineArgs);
                    }
                    else
                    {
                        CommandDrawLine command = new CommandDrawLine();
                        command.Add(lineArgs);
                        currentList.Add(command);
                    }
                }
            }
        }

        public void Add(FillEventArgs fillArgs)
        {
            lock (stack)
            {
                stack.LastOrDefault()?.Add(new CommandFill(fillArgs));
            }
        }

        public IEnumerable<IDrawCommand> Undo()
        {
            lock (stack)
            {
                if (stack.Count == 0)
                {
                    return new List<IDrawCommand>();
                }

                CommandSubList? currentList = stack.LastOrDefault();
                if (currentList?.Count == 0)
                {
                    if (stack.Count == 1)
                    {
                        return new List<IDrawCommand>();
                    }
                    else
                    {
                        stack.Remove(currentList);
                        currentList = stack.LastOrDefault();
                        if (currentList == null)
                        {
                            return new List<IDrawCommand>();
                        }
                    }
                }
                else
                {
                    currentList?.RemoveLast();
                }

                List<IDrawCommand> result = currentList?.ToList() ?? new();

                result.Insert(0, new CommandClearCanvas(currentList?.BackgroundColor ?? CanvasSettings.DEFAULT_BACKGROUND_COLOR));
                return result;
            }
        }

        public IEnumerable<IDrawCommand>? GetDrawCommands()
        {
            return stack.LastOrDefault()?.ToList();
        }


        public IEnumerator<IDrawCommand> GetEnumerator()
        {
            foreach(CommandSubList subList in stack)
            {
                yield return new CommandClearCanvas(subList.BackgroundColor);
                foreach (IDrawCommand command in subList)
                {
                    yield return command;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
