using Application.Helpers;
using Application.Persistence.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Application.SignalR
{
    public class UserHub : Hub
    {
        private readonly MemberConnections _memberConnection;
        private readonly UnitOfWork _unitOfWork;

        public UserHub(MemberConnections userCons, UnitOfWork unitOfWork)
        {
            _memberConnection = userCons;
            _unitOfWork = unitOfWork;
        }

        // public async Task SendNotification(String type, String targetId, String title, String content)
        // {
        //   var httpContext = Context.GetHttpContext();

        //   var userId = _userCons.GetUserIdFromConnection(Context.ConnectionId);
        //   var connections = _userCons.GetUserConnections(userId);

        //   // await Clients.All.SendAsync("NewComment", comment.Value);
        //   foreach (var c in connections)
        //   {
        //     var client = Clients.Client(c);
        //     await client.SendAsync("NewNotification", new
        //     {
        //       type = type,
        //       targetId = targetId,
        //       title = title,
        //       content = content,
        //     });
        //   }
        // }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext!.User.GetId();

            var member = _unitOfWork.MemberRepository.GetQuery().FirstOrDefault(m => m.UserId == userId);
            if (member == null)
            {
                await Clients.Caller.SendAsync("Disconnect", "Connection Disconnected!");
                Context.Abort();
                return;
            }

            _memberConnection.AddConnection(Context.ConnectionId, member.MemberId);
            await Clients.Caller.SendAsync("Connect", "Welcome to Unicare!");

            // await Clients.Caller.SendAsync("LoadComments", result.Value);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            _memberConnection.RemoveConnection(Context.ConnectionId);
        }
    }
}