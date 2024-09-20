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
        private readonly ICollection<String> _places;

        public GameHub(ICollection<Message> messages, ICollection<Game> games, ICollection<Player> players)
        {
            this._games = games;
            this._players = players;
            this._messages = messages;
            this._places = new HashSet<String>() {"Airplane", "Bank", "Beach", "Hospital", "School",
                "Restaurant", "Train", "Supermarket", "Football Stadium", "Park", "Cinema", "Hotel", "Outer Space", "Museum", "Boat"};
        }

        public async Task CreateRoom(UserConnectionDto userConnection)
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
            .SendAsync("CreateRoom", g.NumberOfPlayers - 1);
            
        }

        public async Task Reconnect(UserConnectionDto userConnection)
        {
            Player? p = _players.Where(x => x.Name!.Equals(userConnection.Name) && x.RoomName!.Equals(userConnection.RoomName)).FirstOrDefault();
            if (p != null)
            {
                p.ConnectionId = Context.ConnectionId;
                await Groups.AddToGroupAsync(p.ConnectionId, p.RoomName!);
            }
            var game = _games.Where(x => x.AccessCode!.Equals(userConnection.RoomName)).FirstOrDefault();
            var players = _players.Where(x => x.RoomName == game!.AccessCode).Select(x => x.Name).ToList();
            int? remainingPlayers = game!.NumberOfPlayers - players.Count();
            var messages = _messages
                .Where(x => x.RoomName!.Equals(userConnection.RoomName))
                .Select(x => new { sender = x.Sender, content = x.Content });
            int? remainingVotes = game.NumberOfPlayers / 2;
            if (game.NumberOfPlayers % 2 == 1) remainingVotes++;
            remainingVotes -= game.NumberOfVotes;
            int? remainingFinalVotes = game.NumberOfPlayers - game.NumberOfFinalVotes;
            await Clients.Caller
            .SendAsync("Refresh", remainingPlayers, messages, p!.IsTheMole, game.Location, players, p.HasVoted, remainingVotes, remainingFinalVotes, p.VotedFor);
        }

        public async Task JoinRoom(UserConnectionDto userConnection)
        {
            var game = _games.Where(x => x.AccessCode!.Equals(userConnection.RoomName)).FirstOrDefault();
            if (game == null)
            {
                await Clients.Caller
                .SendAsync("GameDoesntExist", "This game doesn't exist");
            }
            else
            {
                int countPlayers = _players.Where(x => x.RoomName!.Equals(game.AccessCode!)).Count();
                if (countPlayers >= game.NumberOfPlayers) {
                    await Clients.Caller
                    .SendAsync("FullGame", "This game is Full");
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
                    await Groups.AddToGroupAsync(p.ConnectionId, game.AccessCode!);
                    if(game.NumberOfPlayers - countPlayers - 1 == 0)
                    {
                        GameStarted(userConnection);
                    }
                    await Clients.Group(game.AccessCode!)
                        .SendAsync("GameLobby", game.NumberOfPlayers - countPlayers - 1);
                }
                    
            }
        }

        public async Task RequestVote(UserConnectionDto userConnection)
        {
            var game = _games.Where(x => x.AccessCode!.Equals(userConnection.RoomName)).FirstOrDefault();
            var player = _players.Where(x => x.Name!.Equals(userConnection?.Name!) && x.RoomName!.Equals(userConnection.RoomName)).FirstOrDefault();
            if (!player!.WantsToVote)
            {
                player.WantsToVote = true;
                game!.NumberOfVotes++;
                int? remainingVotes = game.NumberOfPlayers / 2;
                if (game.NumberOfPlayers % 2 == 1) remainingVotes++;
                remainingVotes -= game.NumberOfVotes;
                if(remainingVotes <= 0)
                {
                    await Clients.Group(game.AccessCode!)
                        .SendAsync("VotePage", game.NumberOfPlayers);
                }
                else
                {
                    await Clients.Group(game.AccessCode!)
                        .SendAsync("VoteRequest", remainingVotes);
                }
                
            }
        }

        public async Task GameStarted(UserConnectionDto userConnection)
        {
            var game = _games.Where(x => x.AccessCode!.Equals(userConnection.RoomName)).FirstOrDefault();
            game!.HasStarted = true;
            var players = _players.Where(x => x.RoomName == userConnection.RoomName).ToList();
            Random random = new Random();
            int mole = random.Next(0, players.Count);
            int location = random.Next(0, 15);
            game.Location = _places.ElementAt(location);
            players[mole].IsTheMole = true;
            var playerss = _players.Where(x => x.RoomName == game.AccessCode).Select(x => x.Name).ToList();
            int? remainingVotes = game.NumberOfPlayers / 2;
            if (game.NumberOfPlayers % 2 == 1) remainingVotes++;
            foreach (Player p in players) {
                if (p.IsTheMole) {
                    await Clients.Client(p.ConnectionId!).SendAsync("LoadMolePage", playerss, remainingVotes);
                }
                else
                {
                    await Clients.Client(p.ConnectionId!).SendAsync("LoadInnocentPage", game.Location, playerss, remainingVotes);
                }
            }
        }

        public async Task SendMessage(UserConnectionDto userConnection, string content)
        {
            Message message = new Message() {
                Sender = userConnection.Name,
                RoomName = userConnection.RoomName,
                Content = content,
                Time = DateTime.Now,
            };
            _messages.Add(message);
            var messages = _messages
                .Where(x => x.RoomName!.Equals(userConnection.RoomName))
                .Select(x => new {sender = x.Sender, content = x.Content});
            await Clients.Group(userConnection.RoomName!)
                        .SendAsync("NewMessage", messages);
        }

        public async Task Vote(UserConnectionDto userConnection, string votedFor)
        {
            Game? game = _games.Where(x => x.AccessCode!.Equals(userConnection.RoomName)).FirstOrDefault();
            Player? player = _players.Where(x => x.Name!.Equals(userConnection.Name) && x.RoomName!.Equals(userConnection.RoomName)).FirstOrDefault();
            if (!player!.HasVoted)
            {
                player.HasVoted = true;
                player.VotedFor = votedFor;
                game!.NumberOfFinalVotes++;
                Player? p = _players.Where(x => x.Name!.Equals(votedFor) && x.RoomName!.Equals(userConnection.RoomName)).FirstOrDefault();
                p!.NumberOfVotes++;
                if(game.NumberOfPlayers == game.NumberOfFinalVotes)
                {
                    await Clients.Group(userConnection.RoomName!)
                        .SendAsync("GameOver");
                }
                await Clients.Group(userConnection.RoomName!)
                        .SendAsync("NewVote", game.NumberOfPlayers-game.NumberOfFinalVotes);
                await Clients.Client(player.ConnectionId!)
                        .SendAsync("Voted", votedFor);
            }
        }
    }
}
