using System.Net;
// Connect four
namespace connect_four;

class Program {
    public static void Main(string[] args) {
        // The program takes one argument, either 'server', 'client-player' or 'client-bot'.
        if (args.Length < 1 || string.IsNullOrEmpty(args[0])) {
            Console.Write("First argument is missing. Use 'server', 'client-player' or 'client-bot'.");
            return;
        }
        switch (args[0])
        {
            case "server":
                // Get server host and port from user input.
                Console.Write($"Enter server host (or leave empty for {Configuration.defaultServerIp}): ");
                string? _host = Console.ReadLine();
                IPAddress host = string.IsNullOrEmpty(_host) ? Configuration.defaultServerIp : IPAddress.Parse(_host);

                Console.Write($"Enter server port (or leave empty for {Configuration.defaultPort}): ");
                string? _port = Console.ReadLine();
                string port = string.IsNullOrEmpty(_port) ? Configuration.defaultPort.ToString() : _port;

                Server server = new(host, int.Parse(port));
                Task serverTask = server.Start();
                serverTask.Wait(); // Wait for the server to return.
                break;

            case "client-player":
                // Start the human client.
                HumanClient client = new();
                Task clientTask = client.InitClient();
                clientTask.Wait(); // Wait for the client to return.
                break;

            case "client-bot":
                // Start the bot client.
                BotClient bot = new();
                Task botTask = bot.InitClient();
                botTask.Wait(); // Wait for the client to return.
                break;

            default:
                Console.WriteLine("Invalid argument. Use 'server', 'client-player' or 'client-bot'.");
                break;
        }
    }
}
