using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draw.Shared.Game
{
    public class WordDTO
    {
        public WordDTO(string word, WordDifficulty difficulty)
        {
            Word = word;
            Difficulty = difficulty;
        }

        public string Word { get; set; }
        public WordDifficulty Difficulty { get; set; }

        public static string GetHintWord(string word)
        {
            string result = "";
            foreach (char c in word)
            {
                if (Char.IsLetter(c))
                {
                    result += '_';
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }
    }
}
