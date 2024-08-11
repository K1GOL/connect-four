using System.Dynamic;

namespace connect_four;

// Class for game state.
public class Game : ICloneable{
  // Game board.
  public Board Board { get; protected set; } = new();

  // Current player.
  public int ToMove { get; protected set; } = 1;

  // Make a move.
  public void MakeMove(int player, int column) {
    CheckLegalMove(player, column);

    // Find the first empty row in the column.
    int row = 0;
    while (Board.GetCell(row, column) != 0) {
      row++;
    }

    // Make the move.
    Board.SetCell(row, column, ToMove);
    ToMove = ToMove == 1 ? 2 : 1;
  }

  public void CheckLegalMove(int player, int column) {
    // Check that it is this player's turn.
    if (player != ToMove) {
      throw new InvalidPlayerException("It is not this player's turn.");
    }

    // Check that the column is valid.
    if (column < 0 || column > 6) {
      throw new InvalidColumnException("Invalid column.");
    }

    // Check that the column is not full.
    if (Board.GetCell(5, column) != 0) {
      throw new IllegalMoveException("Column is full.");
    }
  }

  // Check if the game is over.
  public bool IsGameOver () {
    return GetWinner() != 0 || IsDraw();
  }

  public bool IsDraw () {
    // Check for draw (there is no legal moves remaining).
    for (int col = 0; col < 7; col++) {
      if (Board.GetCell(5, col) == 0) {
        return false;
      }
    }
    return true;
  }

  // Get the winner of the game.
  // Returns 0 if there is no winner (game in progress or draw).
  public int GetWinner () {
    // Find horizontal wins.
    for (int row = 0; row < 6; row++) {
      for (int col = 0; col < 4; col++) {
        int player = Board.GetCell(row, col);
        if (player != 0 && player == Board.GetCell(row, col + 1) && player == Board.GetCell(row, col + 2) && player == Board.GetCell(row, col + 3)) {
          return player;
        }
      }
    }

    // Find vertical wins.
    for (int col = 0; col < 7; col++) {
      for (int row = 0; row < 3; row++) {
        int player = Board.GetCell(row, col);
        if (player != 0 && player == Board.GetCell(row + 1, col) && player == Board.GetCell(row + 2, col) && player == Board.GetCell(row + 3, col)) {
          return player;
        }
      }
    }

    // Find right diagonal wins.
    for (int row = 0; row < 3; row++) {
      for (int col = 0; col < 4; col++) {
        int player = Board.GetCell(row, col);
        if (player != 0 && player == Board.GetCell(row + 1, col + 1) && player == Board.GetCell(row + 2, col + 2) && player == Board.GetCell(row + 3, col + 3)) {
          return player;
        }
      }
    }

    // Find left diagonal wins.
    for (int row = 0; row < 3; row++) {
      for (int col = 3; col < 7; col++) {
        int player = Board.GetCell(row, col);
        if (player != 0 && player == Board.GetCell(row + 1, col - 1) && player == Board.GetCell(row + 2, col - 2) && player == Board.GetCell(row + 3, col - 3)) {
          return player;
        }
      }
    }

    return 0;
  }

  // Encodes the game state to a byte array.
  public byte[] Encode() {
    // Encodes the game state to the following format:
    // [PlayerToMove 1B][BoardState 42B]
    byte[] encoded = new byte[1 + 42];
    encoded[0] = (byte)ToMove;
    byte[] boardState = this.Board.Encode();
    for (int i = 0; i < 42; i++) {
      encoded[i + 1] = boardState[i];
    }
    return encoded;
  }

  // Decodes the game state from a byte array.
  public void FromBytes(byte[] bytes) {
    Board.FromBytes(bytes);
    ToMove = bytes[0];    
  }

  public void Reset() {
    this.Board = new();
    this.ToMove = 1;
  }

  public object Clone() {
        return new Game()
        {
            Board = (Board)this.Board.Clone(),
            ToMove = this.ToMove
        };
  }
}

// Class for game board.
public class Board : ICloneable {
  // Game board is a two dimensional array of ints.
  // 0 represents an empty cell, 1 represents a player 1 cell and 2 represents a player 2 cell.
  // 6 rows and 7 columns in the following arrangement:
  //   0 1 2 3 4 5 6
  // 5               |
  // 4               |
  // 3               |              
  // 2       *       |
  // 1 O   O O       |
  // 0 *   * O *     |
  //   - - - - - - -
  public int[,] gameBoard { get; private set; } = new int[6, 7];

  public int GetCell(int row, int column) {
    return gameBoard[row, column];
  }

  public void SetCell(int row, int column, int value) {
    gameBoard[row, column] = value;
  }

  // Encodes the game board to a byte array.
  public byte[] Encode() {
    byte[] encoded = new byte[6 * 7];
    for (int row = 0; row < 6; row++) {
      for (int col = 0; col < 7; col++) {
        encoded[row * 7 + col] = (byte)gameBoard[row, col];
      }
    }
    return encoded;
  }

  // Decodes the game board from a byte array.
  public void FromBytes(byte[] bytes) {
    // First byte is the player to move.
    // Skip it.
    for (int row = 0; row < 6; row++) {
      for (int col = 0; col < 7; col++) {
        gameBoard[row, col] = bytes[row * 7 + col + 1];
      }
    }
  }

  public object Clone() {
    return new Board()
    {
      gameBoard = (int[,])this.gameBoard.Clone()
    };
  }
}

// Base class for exceptions thrown by the game.
class GameException : Exception {
  public GameException(string message) : base(message) { }
}

// Invalid player exception.
class InvalidPlayerException : GameException {
  public InvalidPlayerException(string message) : base(message) { }
}

// Invalid column exception.
class InvalidColumnException : GameException {
  public InvalidColumnException(string message) : base(message) { }
}

// Illegal move exception.
class IllegalMoveException : GameException {
  public IllegalMoveException(string message) : base(message) { }
}