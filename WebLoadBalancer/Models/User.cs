using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebLoadBalancer.Models
{
    [Table("users", Schema ="public")]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string User_password { get; set; }
    }
}
