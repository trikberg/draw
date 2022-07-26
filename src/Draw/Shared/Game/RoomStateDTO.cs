using System.Collections.Generic;

namespace Draw.Shared.Game
{
    public class RoomStateDTO
    {
        public RoomStateDTO() { }

        public RoomStateDTO(string roomName, List<PlayerDTO> players, RoomSettings roomSettings)
        {
            RoomName = roomName;
            Players = players;
            RoomSettings = roomSettings;
        }

        public RoomStateDTO(string roomName, List<PlayerDTO> players, RoomSettings roomSettings, bool gameInProgress)
        {
            RoomName = roomName;
            Players = players;
            RoomSettings = roomSettings;
            GameInProgress = gameInProgress;
        }

        public string RoomName { get; set; } = string.Empty;
        public List<PlayerDTO> Players { get; set; } = new();
        public RoomSettings RoomSettings { get; set; } = new();
        public bool GameInProgress { get; set; }
    }
}
