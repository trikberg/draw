using Draw.Shared.Game;

namespace Draw.Server.Game
{
    internal class Word
    {
        public Word(string word, WordDifficulty difficulty)
        {
            TheWord = word;
            Difficulty = difficulty;
        }

        public string TheWord { get; private set;  }
        public WordDifficulty Difficulty { get; private set; }

        internal WordDTO ToWordDTO()
        {
            return new WordDTO(TheWord, Difficulty);
        }
    }
}