using System;

namespace Draw.Shared.Game
{
    public class PlayerGuids
    {
        public PlayerGuids(Guid playerGuid, Guid connectionGuid)
        {
            PlayerGuid = playerGuid;
            ConnectionGuid = connectionGuid;
        }

        public Guid PlayerGuid { get; set; }

        public Guid ConnectionGuid { get; set; }
    }
}
