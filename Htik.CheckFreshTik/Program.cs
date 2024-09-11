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
            Console.OutputEncoding = Encoding.UTF8;

            string filePath = "usernames.txt";
            string freshFilePath = "fresh.txt"; 
            string dieFilePath = "die.txt";
            string unknowFilePath = "unknown.txt";
            string errorFilePath = "error.txt";

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
            File.WriteAllText(freshFilePath, string.Empty);
            File.WriteAllText(dieFilePath, string.Empty);
            File.WriteAllText(unknowFilePath, string.Empty); 
            File.WriteAllText(errorFilePath, string.Empty);

            // Read all lines (usernames) from the file
            var usernames = File.ReadAllLines(filePath);

            int count = 0;
            int freshCount = 0;
            int dieCount = 0;
            int unknowCount = 0;
            int errorCount = 0;
            // Create an HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1");

                foreach (var username in usernames)
                {
                    count += 1;
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
                                    Console.WriteLine($"{count} Sống: {username}");

                                    // Write the username to the output file
                                    File.AppendAllText(freshFilePath, $"{username}{Environment.NewLine}");
                                    freshCount++;
                                }
                                else if(htmlContent.Contains("\"webapp.user-detail\":{\"statusCode\":10221"))
                                {
                                    Console.WriteLine($"{count} Chết: {username}");
                                    File.AppendAllText(dieFilePath, $"{username}{Environment.NewLine}");
                                    dieCount++;
                                }
                                else
                                {
                                    Console.WriteLine($"{count} Không xác định: {username}");
                                    File.AppendAllText(unknowFilePath, $"{username}{Environment.NewLine}");
                                    unknowCount++;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Lỗi khi lướt tiktok {username}: {response.StatusCode}");
                                File.AppendAllText(errorFilePath, $"{username}{Environment.NewLine}");
                                errorCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception {username}: {ex.Message}");
                        File.AppendAllText(errorFilePath, $"{username}{Environment.NewLine}");
                        errorCount++;
                    }
                }
            }

            Console.WriteLine($"Hoàn thành check Sống: ${freshCount}, chết: ${dieCount}, không xác định: ${unknowCount}, lỗi: ${errorCount}.");
            Console.ReadKey();
        }
    }
}
