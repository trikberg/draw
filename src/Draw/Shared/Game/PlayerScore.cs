using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draw.Shared.Game
{
    public class PlayerScore
    {
        public PlayerScore(PlayerDTO player, int score)
        {
            Player = player;
            Score = score;
        }

        public PlayerDTO Player { get; set; }
        public int Score { get; set; }
    }
}
