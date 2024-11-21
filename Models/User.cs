using Claimed.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public bool IsAdmin { get; set; } // False for normal users, true for admins
    public ICollection<Claim> Claims { get; set; }
}
