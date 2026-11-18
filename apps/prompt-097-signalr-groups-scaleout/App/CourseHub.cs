using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
[Authorize]
public sealed class CourseHub : Hub
{
    private void DemandRoom(string room)
    {
        if(room!="dotnet" || !Context.User!.HasClaim("course",room))throw new HubException("Room access denied");
    }
    public async Task Join(string room) {DemandRoom(room);await Groups.AddToGroupAsync(Context.ConnectionId,room);}
    [Authorize(Roles="instructor")]
    public Task Publish(string room,string message) {DemandRoom(room);return Clients.Group(room).SendAsync("Notice",message);}
}
