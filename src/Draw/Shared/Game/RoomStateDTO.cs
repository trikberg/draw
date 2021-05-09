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

        public string RoomName { get; set; }
        public List<PlayerDTO> Players { get; set; }
        public RoomSettings RoomSettings { get; set; }
        public bool GameInProgress { get; set; }
    }
}
