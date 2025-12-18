namespace FoutloosTypen.App.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int Speed { get; set; }
        public double Accuracy { get; set; }
        public int Score { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}