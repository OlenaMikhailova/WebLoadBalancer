using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebLoadBalancer.Models
{
    public class web_user 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int user_id { get; set; }
        public string email { get; set; }
        public string user_password { get; set; }
        public string username { get; set; }
    }
}
