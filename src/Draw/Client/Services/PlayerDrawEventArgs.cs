using Draw.Shared.Game;

namespace Draw.Client.Services
{
    public class PlayerDrawEventArgs
    {

        public PlayerDrawEventArgs(PlayerDTO player, string wordHint, int time)
        {
            this.Player = player;
            this.WordHint = wordHint;
            this.Time = time;
        }

        public PlayerDTO Player { get; }
        public string WordHint { get; }
        public int Time { get; }

    }
}