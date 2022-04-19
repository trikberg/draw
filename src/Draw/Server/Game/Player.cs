using Draw.Shared.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Draw.Server.Game
{
    public class Player
    {
        public Player(string connectionId)
        {
            ConnectionId = connectionId;
            Id = Guid.NewGuid();
            ConnectionGuid = Guid.NewGuid();
            IsConnected = true;
        }

        public Player(string connectionId, string name) : this(connectionId)
        {
            Name = name;
        }

        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Guid Id { get; }
        public Guid ConnectionGuid { get; }
        public bool IsConnected { get; set; }

        internal PlayerDTO ToPlayerDTO()
        {
            return new PlayerDTO(Name, Id, IsConnected);
        }

        public override bool Equals(object o)
        {
            if (o is Player p)
            {
                return ConnectionId.Equals(p.ConnectionId);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
