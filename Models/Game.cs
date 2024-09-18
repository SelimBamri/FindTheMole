namespace FindTheMole.Models
{
    public class Game
    {
        public string? AccessCode { get; set; }
        public int? NumberOfPlayers { get; set; }
        public string? Location { get; set; }
        public int NumberOfVotes { get; set; }
        public bool HasStarted { get; set; }
        public bool HasFinished { get; set; }
    }
}
