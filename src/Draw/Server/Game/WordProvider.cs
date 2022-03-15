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
                int wordCount = 0;

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
                            if (ParseLine(reader.ReadLine(), language))
                            {
                                wordCount++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Exception when reading word list " + languageName);
                    continue;
                }

                logger.Info("Added " + wordCount + " words for language " + languageName);
            }
        }

        private bool ParseLine(string line, Language language)
        {
            string[] split = line.Split(',');
            if (split.Length == 2 &&
                !String.IsNullOrWhiteSpace(split[0]) &&
                Int32.TryParse(split[1].Trim(), out int difficulty))
            {
                Word w = new Word(split[0].Trim(), difficulty);
                WordLists[language].Add(w);
                return true;
            }
            return false;
        }

        internal List<Word> GetWords(Language language, WordDifficulty minDifficulty, WordDifficulty maxDifficulty)
        {
            List<Word> words = WordLists[language].Where(w => w.Difficulty >= minDifficulty && w.Difficulty <= maxDifficulty).ToList();
            words.Shuffle();
            return words;
        }
    }
}