using Draw.Server.Extensions;
using Draw.Shared.Game;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Draw.Server.Game
{
    internal class WordProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly WordProvider instance = new WordProvider();
        public static WordProvider Instance => instance;

        private Dictionary<Language, List<Word>> WordLists = new Dictionary<Language, List<Word>>();

        private WordProvider()
        {
            LoadWordLists();
        }

        private void LoadWordLists()
        {
            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                WordLists.Add(language, new List<Word>());
                string languageName = Enum.GetName(typeof(Language), language);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream resourceStream = assembly.GetManifestResourceStream("Draw.Server.Resources." + languageName + ".csv");

                if (resourceStream == null)
                {
                    logger.Error("Error getting resource stream for word list " + languageName);
                    continue;
                }

                try
                {
                    using (StreamReader reader = new StreamReader(resourceStream, Encoding.UTF8))
                    {
                        while (!reader.EndOfStream)
                        {
                            ParseLine(reader.ReadLine(), language);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Exception when reading word list " + languageName);
                    continue;
                }
            }
        }

        private void ParseLine(string line, Language language)
        {
            string[] split = line.Split(',');
            if (split.Length == 2 &&
                !String.IsNullOrWhiteSpace(split[0]) &&
                Int32.TryParse(split[1].Trim(), out int difficulty))
            {
                Word w = new Word(split[0].Trim(), difficulty);
                WordLists[language].Add(w);
            }
        }

        internal List<Word> GetWords(Language language, WordDifficulty minDifficulty, WordDifficulty maxDifficulty)
        {
            //List<Word> words = new List<Word>();
            //words.Add(new Word("asterix", 1));
            //words.Add(new Word("banana", 2));
            //words.Add(new Word("cat", 3));
            //words.Add(new Word("duck", 4));
            //words.Add(new Word("extra cheese", 5));
            //words.Add(new Word("flying pig", 6));
            //words.Add(new Word("grease", 7));
            //words.Add(new Word("hat in the cat", 8));
            //words.Add(new Word("industry", 9));

            List<Word> words = WordLists[language].Where(w => w.Difficulty >= minDifficulty && w.Difficulty <= maxDifficulty).ToList();
            words.Shuffle();
            return words;
        }
    }
}