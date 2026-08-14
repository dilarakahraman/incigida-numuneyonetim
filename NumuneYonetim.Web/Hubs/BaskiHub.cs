using Microsoft.AspNetCore.SignalR;

namespace NumuneYonetim.Web.Hubs;

public class BaskiHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "EtiketYazicilari");
        await base.OnConnectedAsync();
    }
}
