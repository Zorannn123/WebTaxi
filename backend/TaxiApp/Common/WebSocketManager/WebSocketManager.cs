using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Common.WebSocketManager
{
    public class WebSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new ConcurrentDictionary<string, WebSocket>();
        public void AddConnection(string connectionId, WebSocket webSocket)
        {
            _connections.TryAdd(connectionId, webSocket);
        }

        public void RemoveConnection(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public WebSocket GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var webSocket);
            return webSocket;
        }

        public IEnumerable<WebSocket> GetAllConnections()
        {
            return _connections.Values;
        }
    }
}
