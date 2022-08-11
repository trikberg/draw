using Draw.Shared.Game;
using System.Collections.Generic;
using System.Linq;

namespace Draw.Server.Game
{
    internal class Word
    {
        public Word(string word, WordDifficulty difficulty)
            : this(word, difficulty, new List<string>())
        {
        }

        public Word(string word, WordDifficulty difficulty, List<string> alternateSpellings)
        {
            TheWord = word;
            Difficulty = difficulty;
            AlternateSpellings = alternateSpellings;
        }

        public string TheWord { get; private set; }
        public WordDifficulty Difficulty { get; private set; }
        public int CharacterCount => TheWord.Count(c => c != ' ');

        public List<string> AlternateSpellings { get; private set;}

        internal WordDTO ToWordDTO()
        {
            return new WordDTO(TheWord, Difficulty);
        }
    }
}