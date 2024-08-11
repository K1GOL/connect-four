// Common client components for both humans and bots.
using System.Net;
using System.Net.Sockets;

namespace connect_four;
public abstract class Client {
  protected int Player { get; set; }
  protected Game CurrentGame { get; } = new();
  protected string ServerIp { get; set; } = Configuration.defaultIp.ToString();
  protected string ServerPort { get; set; } = Configuration.defaultPort.ToString();

  public void UpdateLocalGameState () {
    // Request the game state from the server.
    Request request = new GetStateRequest();
    Response response = SendRequest(request);
    ResponseHandler(response);
  }

  protected void ResponseHandler (Response response) {
    switch(response.Type) {
      case ResponseType.SendGameState:
        // Update the game state.
        CurrentGame.FromBytes(response.Data);
        break;
      case ResponseType.Error:
        // Throw error.
        throw new Exception($"Error {((ErrorResponse)response).StatusCode}.");
      default:
        throw new Exception("Invalid response type.");
    }
  }

  // Make a move and update the game state on the server.
  protected void UpdateRemoteGameState (int column) {
    Request request = new UpdateStateRequest((byte)Player, (byte)column);
    Response response = SendRequest(request);
    ResponseHandler(response);
  }

  private Response SendRequest (Request request) {
    // Send a request to the server.
    IPEndPoint endpoint = new(IPAddress.Parse(ServerIp), int.Parse(ServerPort));
    TcpClient client = new();
    client.Connect(endpoint);

    NetworkStream stream = client.GetStream();
    stream.Write(request.Encode(), 0, request.Encode().Length);

    // Read response from server.
    byte[] headers = new byte[3];
    stream.Read(headers, 0, headers.Length);
    int contentLength = headers[2];

    byte[] data = new byte[contentLength];
    stream.Read(data, 0, data.Length);

    client.Close();
    // Concatenate headers with data.
    byte[] bytes = new byte[headers.Length + contentLength];
    Buffer.BlockCopy(headers, 0, bytes, 0, headers.Length);
    Buffer.BlockCopy(data, 0, bytes, headers.Length, contentLength);

    return Decoder.DecodeResponse(bytes);
  }

  abstract public Task GameLoop();

  public async Task InitClient () {
    // Set up the client.
    Terminal.Clear();
    Terminal.ShowCursor();
    Terminal.WriteLine(0, "[ CONNECT FOUR ]");

    // Ask for server details.
    Terminal.WriteLine(2, "Connect to server");
    Terminal.WriteLine(3, $"Server IP address (or leave empty for {ServerIp}) ");
    Terminal.WriteLine(4, ">> ");

    string? _ip = Console.ReadLine();
    this.ServerIp = string.IsNullOrEmpty(_ip) ? ServerIp : _ip;

    Terminal.ClearLines(3, 4);
    Terminal.WriteLine(3, $"Server port (or leave empty for {ServerPort}) ");
    Terminal.WriteLine(4, ">> ");

    string? _port = Console.ReadLine();
    this.ServerPort = string.IsNullOrEmpty(_port) ? ServerPort : _port;

    Terminal.ClearLines(3, 4);
    Terminal.WriteLine(3, "Choose player (1/2)");
    Terminal.WriteLine(4, ">> ");
    string? _input = Console.ReadLine();
    this.Player = string.IsNullOrEmpty(_input) ? 1 : int.Parse(_input);

    Terminal.ClearLines(2, 4);
    // Try to connect.
    Terminal.WriteLine(2, "Connecting...");
    try {
      UpdateLocalGameState();
      Terminal.WriteLine(3, "Connected!");
      await Task.Delay(2000);
      Terminal.ClearLines(1, 4);
      await GameLoop();
    } catch (Exception e) {
      Terminal.WriteLine(3, $"Error: {e.Message} > {e.StackTrace}");
    }
  }
}