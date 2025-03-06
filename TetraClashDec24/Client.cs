using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public static class Client
    {
        public static async Task<string> SendMessageAsync(NetworkStream stream, string message, bool response = false)
        {
            try 
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                Console.WriteLine($"Sent: {message}");
                if (response)
                {
                    string responseMessage = "";
                    byte[] responseBuffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                    responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                    Console.WriteLine(response);

                    return responseMessage.Replace("\n", "");
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return ex.Message;
            }
        }
    }
}