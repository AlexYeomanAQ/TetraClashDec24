using System;
using System.Net.Sockets;
using System.Text;
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

                        byte[] responseBuffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                        string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return "Error";
            }
        }
    }
}
