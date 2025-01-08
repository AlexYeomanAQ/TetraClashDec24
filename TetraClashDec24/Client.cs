using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    class Client
    {
        public static string sendMessage(string message)
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 12345);
                Console.WriteLine("Connected to server.");

                NetworkStream stream = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                // Send the message to the server
                stream.Write(buffer, 0, buffer.Length);
                Console.WriteLine($"Sent: {message}");

                // Optionally, read the server's response
                byte[] responseBuffer = new byte[1024];
                int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                // Close the connection
                stream.Close();
                client.Close();
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return "Error";
            }
        }
    }
}
