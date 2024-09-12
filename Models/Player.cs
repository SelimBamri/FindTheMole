namespace FindTheMole.Models
{
    public class Player
    {
        public string? Name { get; set; }
        public string? RoomName { get; set; }
        public bool IsTheMole { get; set; }
        public bool HasGuessed { get; set; }
        public bool WantsToVote { get; set; }
        public bool HasVoted { get; set; }

    }
}
