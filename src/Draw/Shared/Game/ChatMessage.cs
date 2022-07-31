namespace Draw.Shared.Game
{
    public enum ChatMessageType
    {
        GameFlow,
        Chat,
        Guess,
        CorrectGuess,
        AlreadyGuessed,
    }

    public class ChatMessage
    {
        public ChatMessage(ChatMessageType messageType, string? playerName, string? message)
        {
            this.MessageType = messageType;
            this.Message = message;
            this.PlayerName = playerName;
        }

        public ChatMessageType MessageType { get; set; }
        public string? Message { get; set;  }
        public string? PlayerName { get; set; }
    }
}
