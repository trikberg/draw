using Draw.Shared.Game;

namespace Draw.Client.Services
{
    public class WordChoiceEventArgs
    {
        public WordChoiceEventArgs(WordDTO word1, WordDTO word2, WordDTO word3, int timeout)
        {
            Word1 = word1;
            Word2 = word2;
            Word3 = word3;
            Timeout = timeout;
        }

        public WordDTO Word1 { get; }
        public WordDTO Word2 { get; }
        public WordDTO Word3 { get; }

        public int Timeout { get; }
    }
}