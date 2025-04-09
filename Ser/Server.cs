using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

class Server
{
    private static TcpListener listener;
    private static ConcurrentQueue<Order> orderQueue = new();
    private static ConcurrentDictionary<Guid, Order> orderLookup = new();

    static void Main()
    {
        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Сервер запущен...");

        Thread processingThread = new(ProcessOrders);
        processingThread.Start();

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new(() => HandleClient(client));
            thread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new(stream, Encoding.UTF8);
        using StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true };

        while (true)
        {
            string? line = reader.ReadLine();
            if (line == null) break;

            if (line.StartsWith("SEND_ORDER"))
            {
                BinaryFormatter formatter = new();
                Order order = (Order)formatter.Deserialize(stream);
                orderQueue.Enqueue(order);
                orderLookup[order.OrderId] = order;
                writer.WriteLine($"ORDER_RECEIVED:{order.OrderId}");
            }
            else if (line.StartsWith("STATUS:"))
            {
                var id = Guid.Parse(line.Split(":")[1]);
                if (orderLookup.TryGetValue(id, out Order? order))
                {
                    string status = order.IsCompleted ? "Completed" : "In Progress";
                    writer.WriteLine($"STATUS:{status}");
                }
                else
                {
                    writer.WriteLine("STATUS:Not Found");
                }
            }
            else if (line.StartsWith("CANCEL:"))
            {
                var id = Guid.Parse(line.Split(":")[1]);
                if (orderLookup.TryRemove(id, out Order? order))
                {
                    orderQueue = new ConcurrentQueue<Order>(orderQueue.Where(o => o.OrderId != id));
                    writer.WriteLine("CANCELLED");
                }
                else
                {
                    writer.WriteLine("CANNOT_CANCEL");
                }
            }
        }
    }

    static void ProcessOrders()
    {
        while (true)
        {
            if (orderQueue.TryDequeue(out Order? order))
            {
                Console.WriteLine($"Обработка заказа {order.OrderId} из ресторана {order.RestaurantName}...");
                Thread.Sleep(order.EstimatedTime);
                order.IsCompleted = true;
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
    }
}
