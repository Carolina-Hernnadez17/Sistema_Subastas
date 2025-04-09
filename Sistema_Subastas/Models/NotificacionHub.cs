using Microsoft.AspNetCore.SignalR;

namespace Sistema_Subastas.Models
{
    public class NotificacionHub : Hub
    {
        public async Task EnviarMensaje(string mensaje)
        {
            await Clients.All.SendAsync("RecibirMensaje", mensaje);
        }
       
    }
}
