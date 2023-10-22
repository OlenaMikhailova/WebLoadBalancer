using System;
using System.Globalization;
using System.IO;

namespace methodGaussa
{
    internal class methodGaussa
    {
        static void Main()
        {
            int n = 1000;

            //double[,] matrix = GenerateRandomMatrix(n);
            //double[] vector = GenerateRandomVector(n);

            //Console.WriteLine("The initial system of equations:");
            //PrintSystem(matrix, vector);

            // Записуємо систему у файл
            //WriteSystemToFile("C:\\Users\\HP\\C#\\methodGaussa\\methodGaussa\\1000equations.txt", matrix, vector);

            //double[] solution = MethodGaussa(matrix, vector);

            //Console.WriteLine("\nSolution:");
            //PrintSolution(solution);

            // Зчитуємо систему з файлу та виконуємо обчислення методом Гауса
            string inputFile = "C:\\Users\\HP\\C#\\methodGaussa\\methodGaussa\\1000equations.txt";
            string outputFile = "C:\\Users\\HP\\C#\\methodGaussa\\methodGaussa\\1000solution.txt";

            double[,] readMatrix;
            double[] readVector;
            ReadSystemFromFile(inputFile, out readMatrix, out readVector);

            double[] readSolution = MethodGaussa(readMatrix, readVector);

            Console.WriteLine("\nSolution from file:");
            PrintSolution(readSolution);

            WriteSolutionToFile(outputFile, readSolution);

            Console.ReadLine();
        }

        static double[,] GenerateRandomMatrix(int n)
        {
            Random rand = new Random();
            double[,] matrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = rand.NextDouble();
                }
            }

            return matrix;
        }

        static double[] GenerateRandomVector(int n)
        {
            Random rand = new Random();
            double[] vector = new double[n];

            for (int i = 0; i < n; i++)
            {
                vector[i] = rand.NextDouble();
            }

            return vector;
        }

        static void PrintSystem(double[,] matrix, double[] vector)
        {
            int n = vector.Length;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(matrix[i, j].ToString("F2", CultureInfo.InvariantCulture) + "\t");
                }
                Console.Write("| " + vector[i].ToString("F2", CultureInfo.InvariantCulture));
                Console.WriteLine();
            }
        }

        static void WriteSystemToFile(string filename, double[,] matrix, double[] vector)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                int n = vector.Length;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        writer.Write(matrix[i, j].ToString("F2", CultureInfo.InvariantCulture) + "\t");
                    }
                    writer.Write("| " + vector[i].ToString("F2", CultureInfo.InvariantCulture));
                    writer.WriteLine();
                }
            }
        }

        static void ReadSystemFromFile(string filename, out double[,] matrix, out double[] vector)
        {
            string[] lines = File.ReadAllLines(filename);
            int n = lines.Length;
            matrix = new double[n, n];
            vector = new double[n];

            for (int i = 0; i < n; i++)
            {
                string[] parts = lines[i].Split('|');
                string[] coefficients = parts[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = double.Parse(coefficients[j], CultureInfo.InvariantCulture);
                }

                vector[i] = double.Parse(parts[1], CultureInfo.InvariantCulture);
            }
        }

        static void WriteSolutionToFile(string filename, double[] solution)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                int n = solution.Length;
                for (int i = 0; i < n; i++)
                {
                    writer.WriteLine("X[" + i + "] = " + solution[i].ToString("F4", CultureInfo.InvariantCulture));
                }
            }
        }


        static double[] MethodGaussa(double[,] matrix, double[] vector)
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

        static void PrintSolution(double[] vector)
        {
            int n = vector.Length;
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine("X[" + i + "] = " + vector[i].ToString("F4", CultureInfo.InvariantCulture));
            }
        }
    }
}
