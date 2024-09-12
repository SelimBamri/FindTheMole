using FindTheMole.Dtos;
using FindTheMole.Models;
using Microsoft.AspNetCore.SignalR;

namespace FindTheMole.Hubs
{
    public class GameHub : Hub
    {
        private readonly ICollection<Player> _players;
        private readonly ICollection<Game> _games;
        private readonly ICollection<Message> _messages;

        public GameHub(ICollection<Message> messages, ICollection<Game> games, ICollection<Player> players)
        {
            this._games = games;
            this._players = players;
            this._messages = messages;
        }

        public async Task JoinRoom(UserConnectionDto userConnection)
        {
            var game = _games.Where(x => x.AccessCode!.Equals(userConnection.RoomName)).FirstOrDefault();
            if (game == null)
            {
                Game g = new Game()
                {
                    NumberOfPlayers = userConnection.Capacity,
                    AccessCode = userConnection.RoomName,
                };
                _games.Add(g);
                Player p = new Player()
                {
                    Name = userConnection.Name,
                    RoomName = userConnection.RoomName,
                    ConnectionId = Context.ConnectionId
                };
                _players.Add(p);
                await Groups.AddToGroupAsync(p.ConnectionId, g.AccessCode!);
                await Clients.Caller
                .SendAsync("ReceiveMessage", $"{g.AccessCode}-{g.NumberOfPlayers-1}");
            }
            else
            {
                if (userConnection.IsNewPlayer == false)
                {
                    Player p = _players.Where(x => x.Name.Equals(userConnection.Name!)).FirstOrDefault();
                    if (p != null)
                    {
                        p.ConnectionId = Context.ConnectionId;

                    }
                }
                else
                {
                    Player p = new Player()
                    {
                        Name = userConnection.Name,
                        RoomName = userConnection.RoomName,
                        ConnectionId = Context.ConnectionId
                    };
                    _players.Add(p);
                }
            }
            
        }
    }
}
