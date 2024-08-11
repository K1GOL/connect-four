// A request than is sent to a server.
using System.Data.SqlTypes;
using System.Net;

namespace connect_four;
public class Request {
  public byte[] Data { get; protected set; } = Array.Empty<byte>();
  protected byte contentLength = 0;
  public RequestType Type { get; protected set; }

  // Encodes the request to a byte array.
  // Request is of the format: [RequestType 1B][ContentLength 1B][Data <CL>B]
  // Length:
  public byte[] Encode () {
    byte[] encoded = new byte[1 + 1 + contentLength];
    encoded[0] = (byte)Type;
    encoded[1] = contentLength;
    for (int i = 0; i < contentLength; i++) {
      encoded[i + 2] = Data[i];
    }
    return encoded;
  }
}

// A request for getting game state.
public class GetStateRequest : Request {
  public GetStateRequest () {
    this.Type = RequestType.GetGameState;
  }
}

// Request for updating game state (playing a move).
public class UpdateStateRequest : Request {
  // Player making the move and column to play in.
  public UpdateStateRequest (byte player, byte column) {
    this.Type = RequestType.UpdateGameState;
    // Data format:
    // [Player 1B][Column 1B]
    this.Data = new byte[] { player, column };
    this.contentLength = 2;
  }
}

// Type of the request, either to get the state of the game or to update the state of the game (make a move).
public enum RequestType : byte {
  GetGameState = 0x01,
  UpdateGameState = 0x02
}

public class Decoder {

  // Decodes the request from a byte array.
  public static Request DecodeRequest (byte[] bytes) {
    // Request is of the format: [RequestType 1B][ContentLength 1B][Data <CL>B]
    // Length:
    if (bytes.Length < 2) {
      throw new Exception("Invalid request. Length too short.");
    }

    RequestType requestType = (RequestType)bytes[0];
    byte contentLength = bytes[1];
    byte[] content = new byte[contentLength];
    for (int i = 0; i < contentLength; i++) {
      content[i] = bytes[i + 2];
    }
        return requestType switch
        {
            RequestType.GetGameState => new GetStateRequest(),
            RequestType.UpdateGameState => new UpdateStateRequest(content[0], content[1]),
            _ => throw new Exception("Invalid request. Unknown request type."),
        };
  }

  // Decodes the response from a byte array.
  public static Response DecodeResponse (byte[] bytes) {
    // Response is of the format: [ResponseType 1B][StatusCode 1B][ContentLength 1B][Data <CL>B]
    // Check length:
    if (bytes.Length < 3) {
      Console.WriteLine(bytes);
      throw new Exception("Invalid response. Length too short.");
    }

    ResponseType responseType = (ResponseType)bytes[0];
    StatusCode statusCode = (StatusCode)bytes[1];
    byte contentLength = bytes[2];
    byte[] content = new byte[contentLength];
    for (int i = 0; i < contentLength; i++) {
      content[i] = bytes[i + 3];
    }
    return responseType switch
    {
        ResponseType.SendGameState => new GameStateResponse(content),
        ResponseType.Error => new ErrorResponse(statusCode),
        _ => throw new Exception("Invalid response. Unknown response type."),
    };
  }
}

// A response from the server.
public class Response {
  public byte[] Data  { get; protected set; } = Array.Empty<byte>();
  protected byte contentLength = 0;
  public byte StatusCode { get; protected set; }
  public ResponseType Type { get; protected set; }

  // Encodes the response to a byte array.
  // Response is of the format: [ResponseType 1B][StatusCode 1B][ContentLength 1B][Data <CL>B]
  // Length:
  public byte[] Encode () {
    byte[] encoded = new byte[1 + 1 + 1 + contentLength];
    encoded[0] = (byte)Type;
    encoded[1] = StatusCode;
    encoded[2] = contentLength;
    for (int i = 0; i < contentLength; i++) {
      encoded[i + 3] = Data[i];
    }
    return encoded;
  }
}

public enum ResponseType : byte {
  Error = 0x01,
  SendGameState = 0x02
}

public enum StatusCode : byte {
  Success = 0x00,
  RequestInvalidPlayer = 0x01,
  RequestInvalidColumn = 0x02,
  RequestIllegalMove = 0x03,
  Error = 0xFF
}

public class ErrorResponse : Response {
  public ErrorResponse (StatusCode statusCode) {
    this.Type = ResponseType.Error;
    this.StatusCode = (byte)statusCode;
  }
}

// Response for sending the game state.
// Game must be encoded as a byte array.
public class GameStateResponse : Response {
  public GameStateResponse (byte[] game) {
    this.Type = ResponseType.SendGameState;
    this.Data = game;
    this.contentLength = (byte)Data.Length;
  }
}