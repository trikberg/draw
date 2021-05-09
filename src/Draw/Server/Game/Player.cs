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
        }

        public Player(string connectionId, string name) : this(connectionId)
        {
            Name = name;
        }

        public string ConnectionId { get; }
        public string Name { get; set; }
        public Guid Id { get; }

        internal PlayerDTO ToPlayerDTO()
        {
            return new PlayerDTO(Name, Id);
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
