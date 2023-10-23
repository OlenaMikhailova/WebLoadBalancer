using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebLoadBalancer.Models
{
    public class EquationSol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int task_id { get; set; }
        public int user_id { get; set; }
        public string equation_name { get; set; }
        public byte[] equation { get; set; }
        public byte[] solution { get; set; }
    }
}
