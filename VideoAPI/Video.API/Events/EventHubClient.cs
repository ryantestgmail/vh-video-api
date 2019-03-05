using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Video.API.Events
{
    public interface IEventHubClient
    {
        Task ParticipantStatusMessage(string email, string status);
        Task HearingStatusMessage(long hearingId, string status);
        Task ConsultationMessage(long hearingId, string requestedBy, string requestedFor, string result);
        Task HelpMessage(long hearingId, string participantName);
    }
    
    [Authorize]
    public class EventHub : Hub<IEventHubClient>
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            await base.OnDisconnectedAsync(exception);
        }
    }
}