using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    // This static class provides a utility method for sending messages
    // over a network stream and optionally waiting for a response.
    public static class Client
    {
        public static async Task<string> SendMessageAsync(NetworkStream stream, string message, bool response = false)
        {
            try
            {
                // Convert the message string to a byte array using UTF8 encoding.
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                // Write the byte array to the network stream asynchronously.
                await stream.WriteAsync(buffer, 0, buffer.Length);
                // Log the sent message.
                Console.WriteLine($"Sent: {message}");

                // If a response is expected from the server...
                if (response)
                {
                    // Create a buffer to store the response bytes.
                    byte[] responseBuffer = new byte[1024];
                    // Asynchronously read the response from the network stream.
                    int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                    // Convert the response bytes into a string using UTF8 encoding.
                    string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                    // Log the 'response' boolean value (this might be intended to log the received responseMessage).
                    Console.WriteLine("Received: " + response);

                    // Return the response message with newline characters removed.
                    return responseMessage.Replace("\n", "");
                }
                else
                {
                    // If no response is expected, return an empty string.
                    return "";
                }
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during the send/receive process.
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Return the exception message as the result.
                return ex.Message;
            }
        }
    }
}
