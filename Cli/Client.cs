using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

class Client
{
    static void Main()
    {
        TcpClient client = new("127.0.0.1", 5000);
        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new(stream, Encoding.UTF8);
        using StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true };

        Console.WriteLine("Введите имя ресторана:");
        string restaurantName = Console.ReadLine();

        Console.WriteLine("Введите описание заказа:");
        string description = Console.ReadLine();

        Order order = new()
        {
            RestaurantName = restaurantName,
            Description = description
        };

        writer.WriteLine("SEND_ORDER");
        BinaryFormatter formatter = new();
        formatter.Serialize(stream, order);

        string? response = reader.ReadLine();
        if (response?.StartsWith("ORDER_RECEIVED:") == true)
        {
            Guid orderId = Guid.Parse(response.Split(":")[1]);
            Console.WriteLine($"Заказ отправлен. ID: {orderId}");

            while (true)
            {
                Console.WriteLine("Проверить статус заказа? (y/n)");
                if (Console.ReadLine()?.ToLower() != "y") break;

                writer.WriteLine($"STATUS:{orderId}");
                string? status = reader.ReadLine();
                Console.WriteLine("Статус: " + status);
            }
        }
    }
}
