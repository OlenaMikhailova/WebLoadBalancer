using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using WebLoadBalancer.Attributes;

namespace WebLoadBalancer.ViewModels
{
    public class EquationSolViewModel
    {
        [Display(Name = "Your file with the system of equations")]
        [Required(ErrorMessage = "Select the file with the system of equations")]
        [DataType(DataType.Upload)]
        [ValidFileExtensions(".txt", ".csv")]
        public IFormFile EquationFile { get; set; }
    }
}
