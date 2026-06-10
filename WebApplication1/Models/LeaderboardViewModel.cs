using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class UserLeaderboardModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int FileCount { get; set; }
    }

    public class LeaderboardViewModel
    {
        public List<StoredFile> TopFiles { get; set; } = new List<StoredFile>();
        public List<UserLeaderboardModel> TopUsers { get; set; } = new List<UserLeaderboardModel>();
    }
}
