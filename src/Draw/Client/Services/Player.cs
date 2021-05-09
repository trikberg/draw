using Draw.Shared.Game;
using System;

namespace Draw.Client.Services
{
    public class Player
    {
        private int score;
        private int? position = null;
        public event EventHandler<int> ScoreChanged;

        internal Player(PlayerDTO player)
        {
            Name = player.Name;
            Id = player.Id;
            Score = 0;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Score
        {
            get { return score; }
            set
            {
                if (value != score)
                {
                    score = value;
                    ScoreChanged?.Invoke(this, score);
                }
            }
        }

        public int? Position
        {
            get { return position; }
            set
            {
                if (value != position)
                {
                    position = value;
                    ScoreChanged?.Invoke(this, score);
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Player p && this.Id.Equals(p.Id))
            {
                return true;
            }

            if (obj is PlayerDTO pdto && this.Id.Equals(pdto.Id))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}