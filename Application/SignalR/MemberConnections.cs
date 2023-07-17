namespace Application.SignalR
{
    public class MemberConnections
    {
        private Dictionary<string, Guid> Connections { get; set; } = new Dictionary<string, Guid>();

        public void AddConnection(string connectionId, Guid userId)
        {
            Connections.Add(connectionId, userId);
        }

        public void RemoveConnection(string connectionId)
        {
            Connections.Remove(connectionId);
        }

        public List<string> GetUserConnections(Guid userId)
        {
            return Connections.Where(p => userId == p.Value).ToList().Select(v => v.Key).ToList();
        }

        public Guid GetUserIdFromConnection(string connectionId)
        {
            return Connections[connectionId];
        }
    }
}