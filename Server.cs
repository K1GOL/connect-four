using System.Net;
using System.Net.Sockets;

namespace connect_four;
class Server {
  private readonly IPAddress host;
  private readonly int port;
  private static readonly Game game = new();
  public Server(IPAddress host, int port) {
    this.host = host;
    this.port = port;
  }

  public async Task Start() {
    Console.WriteLine($"Connect four server v{Configuration.version}");
    Console.WriteLine($"Starting server {host}:{port}...");
    // Starts the server.
    IPEndPoint endpoint = new(host, port);
    TcpListener listener = new(endpoint);   

    try {
      listener.Start();
      Console.WriteLine($"Server started on {host}:{port}");
      while (true) {
        Console.WriteLine("Waiting for connection...");
        using TcpClient handler = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Incoming connection...");
        HandleConnection(handler);

        // Check if game is over.
        if (game.IsGameOver()) {
          Console.WriteLine("Game is over.");
          ResetHandler();
        }
      } 
    } catch (Exception e) {
      Console.WriteLine($"Server has encourtered an error: {e.Message} > {e.StackTrace}");
    } finally {
      listener.Stop();
    }
  }

  public static async void ResetHandler() {
    await Task.Delay(8000);
    Console.WriteLine("Restarting game.");
    game.Reset();
  }

  private static void HandleConnection(TcpClient handler) {
    // Handles the connection.
    NetworkStream stream = handler.GetStream();
    Console.WriteLine($"Connection from {handler.Client.RemoteEndPoint}");

    // Read request headers.
    byte[] headers = new byte[2];
    stream.Read(headers, 0, headers.Length);
    int contentLength = headers[1];

    // Read request data.
    byte[] data = new byte[contentLength];
    if (contentLength > 0) {
      stream.Read(data, 0, data.Length);
      Console.WriteLine("Read data.");
    }

    // Decode the request.
    // Concatenate the headers and data together.
    byte[] bytes = new byte[headers.Length + contentLength];
    Buffer.BlockCopy(headers, 0, bytes, 0, headers.Length);
    Buffer.BlockCopy(data, 0, bytes, headers.Length, contentLength);
    Request request = Decoder.DecodeRequest(bytes);
    Console.WriteLine($"Request type: {request.Type}");

    // Handle request.
    switch (request.Type) {
      case RequestType.GetGameState:
        // Send game state to the client.
        SendResponse(stream, new GameStateResponse(game.Encode()));
        break;
      case RequestType.UpdateGameState:
        // Try to make a move.
        int player = request.Data[0];
        int column = request.Data[1];
        Console.WriteLine($"Player {player} is moving to column {column}");

        StatusCode status = TryMove(player, column);
        if (status == StatusCode.Success) {
          Console.WriteLine($"Move successful.");
          SendResponse(stream, new GameStateResponse(game.Encode()));
        } else {
          Console.WriteLine($"Move failed: {status}");
          SendResponse(stream, new ErrorResponse(status));
        }
        break;
      default:
        Console.WriteLine($"Invalid request type: {request.Type}");
        break;
    }
  }

  // Tries to make a move. Returns appropriate status code.
  private static StatusCode TryMove(int player, int column) {
    StatusCode status;
    try {
      game.MakeMove(player, column);
      status = StatusCode.Success;
    } catch (Exception e) {
      status = e switch
      {
        InvalidPlayerException => StatusCode.RequestInvalidPlayer,
        InvalidColumnException => StatusCode.RequestInvalidColumn,
        IllegalMoveException => StatusCode.RequestIllegalMove,
        _ => StatusCode.Error,
      };
    }
    return status;
  }

  private static void SendResponse(NetworkStream stream, Response response) {
    // Send a response to the client.
    byte[] reponseBytes = response.Encode();
    Console.WriteLine($"Sending bytes: {string.Join(" ", reponseBytes)}");
    stream.Write(reponseBytes, 0, reponseBytes.Length);
  }

}