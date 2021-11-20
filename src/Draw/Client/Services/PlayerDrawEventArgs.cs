using Draw.Shared.Game;

namespace Draw.Client.Services
{
    public class PlayerDrawEventArgs
    {

        public PlayerDrawEventArgs(PlayerDTO player, int time)
        {
            this.Player = player;
            this.Time = time;
        }

        public PlayerDTO Player { get; }
        public int Time { get; }

    }
}