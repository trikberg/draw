using Draw.Shared.Game;
using System.Linq;

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
        public int CharacterCount => TheWord.Count(c => c != ' ');

        internal WordDTO ToWordDTO()
        {
            return new WordDTO(TheWord, Difficulty);
        }
    }
}