using System.ComponentModel.DataAnnotations;

namespace WebLoadBalancer.ViewModels
{
    public class EquationSolViewModel
    {
        [Display(Name = "Your file with the system of equations")]
        [Required(ErrorMessage = "Select the file with the system of equations")]
        [DataType(DataType.Upload)]
        [ValidFileExtensions(10485760, ".txt", ".csv")]
        public IFormFile EquationFile { get; set; }

        
    }
}
