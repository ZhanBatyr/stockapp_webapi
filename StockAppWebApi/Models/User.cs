using System.ComponentModel.DataAnnotations;

namespace StockAppWebApi.Models
{
    public class User
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public byte[] Password { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool isGuest { get; set; } = true;

        public User(string id, string name, string email, byte[] password)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
        }

        public User(string id, string name, string email, byte[] password, byte[] salt)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            PasswordSalt = salt;
        }
    }
}
