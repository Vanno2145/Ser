// Client.cs
using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main()
    {
        const string serverIP = "127.0.0.1";
        const int port = 9000;

        Console.Write("Что хотите запросить? (time/date): ");
        string userInput = Console.ReadLine()?.Trim().ToLower();

        if (userInput != "time" && userInput != "date")
        {
            Console.WriteLine(" Неверный ввод. Введите 'time' или 'date'.");
            return;
        }

        try
        {
            using (TcpClient client = new TcpClient(serverIP, port))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] requestData = Encoding.UTF8.GetBytes(userInput);
                stream.Write(requestData, 0, requestData.Length);

                byte[] buffer = new byte[256];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Ответ от сервера: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠ Ошибка подключения: " + ex.Message);
        }
    }
}
