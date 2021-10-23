using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Draw.Client.Services
{
    public class LoggerService : ILoggerService
    {
        private HubConnection hubConnection;

        public LoggerService(NavigationManager navigationManager)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/log"))
                .Build();
            _ = hubConnection.StartAsync();
        }

        public Task Fatal(Exception exception)
        {
            return hubConnection.InvokeAsync("Fatal", exception.GetType().ToString(), exception.StackTrace, exception.Message);
        }
    }
}
