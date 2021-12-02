using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Draw.Server.Hubs
{
    public class ClientLogHub : Hub
    {
        public const string HubUrl = "/log";

        private ILogger<ClientLogHub> logger;

        public ClientLogHub(ILogger<ClientLogHub> logger)
        {
            this.logger = logger;
        }

        public void Fatal(string type, string stacktrace, string message)
        {
            logger.LogError("Client unhandled exception: " + message + "\n" + type + "\n" + stacktrace);
        }
    }
}
