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
        public static async Task<string> SendMessageAsync(NetworkStream stream, string message)
        {
            try {
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
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return ex.Message;
            }
        }
    }
}