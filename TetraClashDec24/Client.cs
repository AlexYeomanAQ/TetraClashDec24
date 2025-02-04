using System;
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

                        return response;
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
            TcpClient client = new TcpClient("localhost", 12345);
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + response);

                    if (response.StartsWith("found:"))
                    {
                        return response.Split(':')[1..];
                    }
                    else
                    {
                        Console.WriteLine(response);
                        return null;
                    }
                }
            }
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
                        Console.WriteLine(gridJson);
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