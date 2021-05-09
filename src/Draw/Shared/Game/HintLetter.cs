namespace Draw.Shared.Game
{
    public class HintLetter
    {
        public HintLetter(int position, char letter)
        {
            Position = position;
            Letter = letter;
        }

        public int Position { get; set; }
        public char Letter { get; set; }
    }
}
