namespace FindTheMole.Models
{
    public class Player
    {
        public string? ConnectionId { get; set; }
        public string? Name { get; set; }
        public string? RoomName { get; set; }
        public string? VotedFor { get; set; }
        public bool IsTheMole { get; set; }
        public bool HasGuessed { get; set; }
        public bool WantsToVote { get; set; }
        public bool HasVoted { get; set; }
        public int NumberOfVotes { get; set; }

    }
}
