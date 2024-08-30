using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Htik.CheckFreshTik
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Define the path to the input file
            string filePath = "usernames.txt"; // Replace with your file path
            string outputFilePath = "fresh.txt";   // Replace with your desired output file path

            // define config
            int delayMs = int.Parse(ConfigurationManager.AppSettings["delayEachLine"]);

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                Console.ReadKey();
                return;
            }

            // Clear the output file by creating or overwriting it with an empty string
            File.WriteAllText(outputFilePath, string.Empty);

            // Read all lines (usernames) from the file
            var usernames = File.ReadAllLines(filePath);

            // Create an HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                foreach (var username in usernames)
                {
                    try
                    {
                        if(!String.IsNullOrWhiteSpace(username))
                        {
                            Thread.Sleep(delayMs);
                            // Construct the TikTok profile URL
                            string url = $"https://www.tiktok.com/@{username}";

                            // Send a GET request
                            HttpResponseMessage response = await client.GetAsync(url);

                            if (response.IsSuccessStatusCode)
                            {
                                // Read and display the HTML content
                                string htmlContent = await response.Content.ReadAsStringAsync();
                                if (htmlContent.Contains("\"webapp.user-detail\":{\"userInfo\""))
                                {
                                    Console.WriteLine($"[data-e2e=\"user-title\"] found for user: {username}");

                                    // Write the username to the output file
                                    File.AppendAllText(outputFilePath, $"{username}{Environment.NewLine}");
                                }
                                else
                                {
                                    Console.WriteLine($"[data-e2e=\"user-title\"] not found for user: {username}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Failed to retrieve {username}: {response.StatusCode}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving {username}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("Processing completed. Check output file for results.");
            Console.ReadKey();
        }
    }
}
