using Draw.Shared.Game;

namespace Draw.Client.Services
{
    public class ActivePlayerDrawEventArgs
    {
        public ActivePlayerDrawEventArgs(WordDTO word, int time)
        {
            this.Word = word;
            this.Time = time;
        }

        public WordDTO Word { get; }
        public int Time { get; }
    }
}