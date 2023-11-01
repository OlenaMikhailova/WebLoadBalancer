using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using WebLoadBalancer.Models;
using WebLoadBalancer.ViewModels;

namespace WebLoadBalancer.Controllers
{
    [Authorize]
    public class EquationSolController : Controller
    {
        private readonly ApplicationContext _context;

        public EquationSolController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult UploadSystem()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadSystem(EquationSolViewModel model)
        {
            if(ModelState.IsValid)
            {
                if (model.EquationFile != null && model.EquationFile.Length > 0)
                {
                    using (var reader = new StreamReader(model.EquationFile.OpenReadStream()))
                    {
                        var lines = reader.ReadToEnd().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                        if (lines.Length > 0)
                        {
                            var matrix = new double[lines.Length, lines.Length];
                            var vector = new double[lines.Length];

                            for (int i = 0; i < lines.Length; i++)
                            {
                                var parts = lines[i].Split('|');
                                var coefficients = parts[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                for (int j = 0; j < lines.Length; j++)
                                {
                                    matrix[i, j] = double.Parse(coefficients[j], CultureInfo.InvariantCulture);
                                }

                                vector[i] = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            }

                            var solution = MethodGaussa(matrix, vector);

                            var userIdClaim = User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
                            if (userIdClaim != null)
                            {
                                string username = userIdClaim.Value;

                                var user = _context.Users.FirstOrDefault(u => u.username == username);

                                if (user != null)
                                {
                                    var equationSol = new EquationSol
                                    {
                                        user_id = user.user_id,
                                        equation_name = model.EquationFile.FileName,
                                        equation = ReadBytesFromIFormFile(model.EquationFile),
                                        solution = WriteSolutionToBytes(solution)
                                    };

                                    _context.EquationSols.Add(equationSol);
                                    await _context.SaveChangesAsync();
                                }

                            }
                        }
                    }
                }
                return RedirectToAction("ViewResults", "EquationSol");
            }
            return View(model);
        }

        private byte[] WriteSolutionToBytes(double[] solution)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                for (int i = 0; i < solution.Length; i++)
                {
                    writer.WriteLine("X[" + i + "] = " + solution[i].ToString("F4", CultureInfo.InvariantCulture));
                }
                writer.Flush();
                return stream.ToArray();
            }
        }

        private byte[] ReadBytesFromIFormFile(IFormFile file)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                file.CopyTo(stream);
                return stream.ToArray();
            }
        }

        private double[] MethodGaussa(double[,] matrix, double[] vector)
        {
            int n = vector.Length;
            double[] solution = new double[n];

            for (int i = 0; i < n; i++)
            {
                if (matrix[i, i] == 0)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        if (matrix[j, i] != 0)
                        {
                            for (int k = i; k < n; k++)
                            {
                                double temp = matrix[i, k];
                                matrix[i, k] = matrix[j, k];
                                matrix[j, k] = temp;
                            }
                            double tempVector = vector[i];
                            vector[i] = vector[j];
                            vector[j] = tempVector;
                            break;
                        }
                    }
                }

                for (int j = i + 1; j < n; j++)
                {
                    double factor = matrix[j, i] / matrix[i, i];
                    for (int k = i; k < n; k++)
                    {
                        matrix[j, k] -= factor * matrix[i, k];
                    }
                    vector[j] -= factor * vector[i];
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                {
                    sum += matrix[i, j] * solution[j];
                }
                solution[i] = (vector[i] - sum) / matrix[i, i];
            }
            return solution;
        }

        [HttpGet]
        public IActionResult ViewResults()
        {
            var userIdClaim = User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
            if (userIdClaim != null)
            {
                string username = userIdClaim.Value;

                var user = _context.Users.FirstOrDefault(u => u.username == username);
                var equations = _context.EquationSols.Where(e => e.user_id == user.user_id).ToList();
                return View(equations);
            }
            return View(new List<EquationSol>());
        }

        [HttpGet]
        public async Task<IActionResult> DownloadSolution(int id)
        {
            var equationSol = await _context.EquationSols.FindAsync(id);

            if (equationSol != null)
            {
                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = "solution.txt",
                    Inline = true,
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(equationSol.solution, "text/plain");
            }
            else
            {
                return NotFound();
            }
        }

    }
}
