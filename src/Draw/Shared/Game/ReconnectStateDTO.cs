using System;

namespace Draw.Shared.Game
{
    public class ReconnectStateDTO
    {
        public ReconnectStateDTO(Guid playerId, RoomStateDTO? roomState)
        {
            PlayerId = playerId;
            RoomState = roomState;
        }

        public Guid PlayerId { get; set; }
        public RoomStateDTO? RoomState { get; set; }
    }
}
