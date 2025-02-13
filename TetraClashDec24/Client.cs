using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public static class Client
    {
        public static async Task<string> SendMessageAsync(string message)
        {
            try
            {
                using (TcpClient client = new TcpClient("localhost", 12345))
                {
                    Console.WriteLine("Connected to server.");
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(message);

                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        Console.WriteLine($"Sent: {message}");


                        string response = "";
                        byte[] responseBuffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                        response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                        Console.WriteLine(response);

                        return response.Replace("\n", "");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return ex.Message;
            }
        }
        public static async Task<string[]> ListenForMatch()
        {
            try
            {
                using TcpClient client = new TcpClient("localhost", 12345);
                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];

                while (true)
                {
                    await Task.Delay(500); // Give some time for data
                    Console.WriteLine($"Bytes available: {client.Available}");

                    if (client.Available > 0) // ✅ Check if there is data before reading
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received: " + response);

                        if (response.StartsWith("found"))
                        {
                            return response.Split(':')[1..];
                        }
                    }
                    else
                    {
                        Console.WriteLine("No data yet...");
                    }
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return Array.Empty<string>();
        }



        public static async Task SendGridAsync(int id, int[,] grid)
        {
            try
            {
                using (TcpClient client = new TcpClient("localhost", 12345))
                {
                    Console.WriteLine("Connected to server.");
                    using (NetworkStream stream = client.GetStream())
                    {
                        string gridJson = JsonSerializer.Serialize(grid);
                        string message = $"grid:{id}:{gridJson}";
                        byte[] buffer = Encoding.UTF8.GetBytes(message);

                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        Console.WriteLine("Sent grid:");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static async Task<int[,]> ReceiveGridAsync()
        {
            TcpClient client = new TcpClient("localhost", 12345);
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string gridJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    int[,] grid = JsonSerializer.Deserialize<int[,]>(gridJson);
                    Console.WriteLine("Received grid:");
                    Console.WriteLine(gridJson);
                    return grid;
                }
            }
        }
    }
}