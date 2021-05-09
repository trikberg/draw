using System;

namespace Draw.Shared.Game
{
    public class WordDifficulty
    {
        public static readonly int MinimumDifficulty = 1;
        public static readonly int MaximumDifficulty = 9;

        public int Difficulty { get; set; }

        public static string DifficultyToColor(int i)
        {
            return i switch
            {
                1 => "#33ff33",
                2 => "#66ff33",
                3 => "#99ff33",
                4 => "#ccff33",
                5 => "#ffff33",
                6 => "#ffcc33",
                7 => "#ff9933",
                8 => "#ff6633",
                9 => "#ff3333",
                _ => throw new ArgumentException("Difficulty outside of valid range."),
            };
        }

        public override string ToString()
        {
            return Difficulty.ToString();
        }

        public static implicit operator WordDifficulty(int i)
        {
            return new WordDifficulty() { Difficulty = i };
        }

        public static implicit operator int(WordDifficulty wd)
        {
            return wd.Difficulty;
        }

        public static bool operator ==(WordDifficulty wd1, WordDifficulty wd2)
        {
            return wd1.Difficulty == wd2.Difficulty;
        }

        public static bool operator !=(WordDifficulty wd1, WordDifficulty wd2)
        {
            return wd1.Difficulty != wd2.Difficulty;
        }

        public static int operator %(WordDifficulty wd1, int i)
        {
            return wd1.Difficulty % i;
        }

        public static bool operator >=(WordDifficulty wd1, WordDifficulty wd2)
        {
            return wd1.Difficulty >= wd2.Difficulty;
        }

        public static bool operator <=(WordDifficulty wd1, WordDifficulty wd2)
        {
            return wd1.Difficulty <= wd2.Difficulty;
        }

        public static string DifficultyToColor(WordDifficulty wd)
        {
            return DifficultyToColor(wd.Difficulty);
        }

        public override bool Equals(object obj)
        {
            if (obj is WordDifficulty wd)
            {
                return this.Difficulty == wd.Difficulty;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
