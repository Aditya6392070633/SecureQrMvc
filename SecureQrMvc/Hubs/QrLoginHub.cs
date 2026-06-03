using Microsoft.AspNetCore.SignalR;

namespace SecureQrMvc.Hubs;

public class QrLoginHub : Hub
{
    public async Task JoinQrSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }
}
