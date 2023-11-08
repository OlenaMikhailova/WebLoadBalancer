using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebLoadBalancer.Hubs;
using WebLoadBalancer.Models;
using WebLoadBalancer.ViewModels;

namespace WebLoadBalancer.Controllers
{
    [Authorize]
    public class EquationSolController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IHubContext<ProgressHub> _hubContext;
        private bool isCalculationCancelled = false;
        private const int MaxEquationCount = 10;

        public EquationSolController(ApplicationContext context, IHubContext<ProgressHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult UploadSystem()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadSystem(EquationSolViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
                if (userIdClaim != null)
                {
                    string username = userIdClaim.Value;
                    var user = _context.Users.FirstOrDefault(u => u.username == username);

                    if (user != null)
                    {
                        var equationsCount = _context.EquationSols
                            .Where(e => e.user_id == user.user_id)
                            .Count();

                        if (equationsCount >= MaxEquationCount)
                        {
                            ModelState.AddModelError("EquationFile", "Досягнута максимальна кількість рівнянь для цього користувача.");
                            return View(model);
                        }
                    }
                }
                if (model.EquationFile != null && model.EquationFile.Length > 0)
                {
                    using (var reader = new StreamReader(model.EquationFile.OpenReadStream()))
                    {
                        var lines = reader.ReadToEnd().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                        bool isEquationFile = true;

                        foreach (var line in lines)
                        {
                            if (!line.Contains("|"))
                            {
                                isEquationFile = false;
                                break;
                            }
                        }

                        if (isEquationFile)
                        {
                            if (lines.Length > 0)
                            {
                                var matrix = new double[lines.Length, lines.Length];
                                var vector = new double[lines.Length];

                                for (int i = 0; i < lines.Length; i++)
                                {
                                    var parts = lines[i].Split('|');
                                    
                                    var coefficients = parts[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                    for (int j = 0; j < coefficients.Length; j++)
                                    {
                                        if (!double.TryParse(coefficients[j], NumberStyles.Any, CultureInfo.InvariantCulture, out matrix[i, j]))
                                        {
                                            ModelState.AddModelError("EquationFile", "The file does not appear to contain a valid equation.");
                                            return View(model);
                                        }
                                    }

                                    if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out vector[i]))
                                    {
                                        ModelState.AddModelError("EquationFile", "The file does not appear to contain a valid equation.");
                                        return View(model);
                                    }

                                    for (int j = 0; j < lines.Length; j++)
                                    {
                                        matrix[i, j] = double.Parse(coefficients[j], CultureInfo.InvariantCulture);
                                    }

                                    vector[i] = double.Parse(parts[1], CultureInfo.InvariantCulture);
                                }

                                
                                var solution = MethodGaussa(matrix, vector);
                                byte[] solutionData = WriteSolutionToBytes(solution);
                                string solutionFileName = Path.GetFileNameWithoutExtension(model.EquationFile.FileName) + "-solution.txt";

                                //var userIdClaim = User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
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
                                            solution = WriteSolutionToBytes(solution),

                                        };


                                        _context.EquationSols.Add(equationSol);
                                        await _context.SaveChangesAsync();

                                    }
                                }
                                if (!isCalculationCancelled)
                                {
                                    return File(solutionData, "application/octet-stream", solutionFileName);
                                }
                                else
                                {
                                    CancelCalculation(model);
                                }



                            }
                            return View(model);
                        }
                        else
                        {
                            ModelState.AddModelError("EquationFile", "The file does not appear to contain a valid equation.");
                            return View(model);
                        }
                    }
                }
                
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelCalculation(EquationSolViewModel model)
        {
            
            var userIdClaim = User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
            if (userIdClaim != null)
            {
                string username = userIdClaim.Value;

                var user = _context.Users.FirstOrDefault(u => u.username == username);

                if (user != null)
                {
                    var lastEquation = _context.EquationSols
                        .Where(e => e.user_id == user.user_id)
                        .OrderByDescending(e => e.task_id)
                        .FirstOrDefault();

                    if (lastEquation != null)
                    {
                        _context.EquationSols.Remove(lastEquation);
                        await _context.SaveChangesAsync();
                        isCalculationCancelled = true;
                    }
                }
            }

            return RedirectToAction("UploadSystem");
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

        private IFormFile WriteBytesToIFormFile(byte[] data, string fileName)
        {
            var stream = new MemoryStream(data);
            var formFile = new FormFile(stream, 0, data.Length, "data", fileName);
            return formFile;
        }

        private double[] MethodGaussa(double[,] matrix, double[] vector)
        {
            int n = vector.Length;
            double[] solution = new double[n];
            int progress;

            if (!isCalculationCancelled)
            {
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
                    progress = (i + 1) * 100 / n;
                    _hubContext.Clients.All.SendAsync("ReceiveProgressUpdate", progress);
                    Console.WriteLine(progress);

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
            else
            {
                return null;
            }
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
        public async Task<IActionResult> DownloadEquation(int id)
        {
            var equationSol = await _context.EquationSols.FindAsync(id);

            if (equationSol != null)
            { 

                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = equationSol.equation_name,
                    Inline = false,
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(equationSol.equation, "text/plain");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadSolution(int id)
        {
            var equationSol = await _context.EquationSols.FindAsync(id);

            if (equationSol != null)
            {
                var equationName = Path.GetFileNameWithoutExtension(equationSol.equation_name);
                var resultFileName = $"{equationName}-solution.txt";

                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = resultFileName,
                    Inline = false,
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                return File(equationSol.solution, "text/plain");
            }
            else
            {
                return NotFound();
            }
        }

        public FileResult DownloadSolutionPlain()
        {
            byte[] solutionData = (byte[])ViewBag.SolutionData;
            return File(solutionData, "application/octet-stream", "solution.txt");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllCalculations()
        {
            var userIdClaim = User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
            if (userIdClaim != null)
            {
                string username = userIdClaim.Value;
                var user = _context.Users.FirstOrDefault(u => u.username == username);

                if (user != null)
                {
                    var equationsToDelete = _context.EquationSols
                        .Where(e => e.user_id == user.user_id)
                        .ToList();

                    _context.EquationSols.RemoveRange(equationsToDelete);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("ViewResults");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCalculation(int id)
        {
            var equationToDelete = await _context.EquationSols.FindAsync(id);

            if (equationToDelete != null)
            {
                _context.EquationSols.Remove(equationToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewResults");
        }

    }
}