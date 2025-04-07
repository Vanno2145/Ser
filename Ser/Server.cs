// Server.cs
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main()
    {
        const int port = 9000;
        var listener = new TcpListener(IPAddress.Any, port);

        listener.Start();
        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        while (true)
        {
            using (TcpClient client = listener.AcceptTcpClient())
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Клиент подключён.");

                byte[] buffer = new byte[256];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).ToLower();

                string response;

                if (request == "time")
                {
                    response = DateTime.Now.ToLongTimeString();
                }
                else if (request == "date")
                {
                    response = DateTime.Now.ToShortDateString();
                }
                else
                {
                    response = "Неверный запрос";
                }

                byte[] responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);

                Console.WriteLine($"Запрос: {request} → Ответ: {response}");
            }

            Console.WriteLine("Соединение закрыто.\n");
        }
    }
}
