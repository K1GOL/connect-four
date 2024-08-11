// Game client for a human player.
using System.Formats.Asn1;
using System.Security.Cryptography;

namespace connect_four;
class HumanClient : Client {

  // Draws the game board.
  private void DrawGame () {
    // Game terminal view layout:
    //  ---------------------
    // 0  [ CONNECT FOUR ]
    // 1
    // 2  Your turn
    // 3   1 2 3 4 5 6 7
    // 4  |             |
    // 5  |             |
    // 6  |             |
    // 7  |*            |
    // 8  |O   O O      |
    // 9  |* O * *      |
    // 10 ----------------
    // 11 
    // 12 Enter column to play:
    // 13 >> 
    // 14
    // 15 v1.2.3

    // Hide cursor while drawing to stop it flying around the board.
    Terminal.HideCursor();

    // Draw fixed elements.
    Terminal.ClearLines(1, 15);

    string symbol = Configuration.symbols[this.CurrentGame.ToMove - 1];

    if (CurrentGame.ToMove == Player) {
      Terminal.WriteLine(2, $"Your < {symbol} > turn");
    } else {
      Terminal.WriteLine(2, $"Opponent's < {symbol} > turn");
    }

    Terminal.WriteLine(3, " 1 2 3 4 5 6 7");

    for (int i = 4; i < 10; i++) {
      Terminal.WriteLine(i, "|             |");
    }

    Terminal.WriteLine(10, "----------------");
    Terminal.WriteLine(12, "Enter column to play:");

    // Draw game board.
    // Rememver to invert rows.
    for (int row = 0; row < 6; row++) {
      for (int column = 0; column < 7; column++) {
        int cellOwner = CurrentGame.Board.GetCell(row, column);
        if (cellOwner != 0) {
          Terminal.WriteChar(5 - row + 4, 1 + column * 2, Configuration.symbols[cellOwner - 1][0]);
        }
      }
    }

    Terminal.WriteLine(15, $"v{Configuration.version}");

    // Draw column prompt last so cursor is at the correct position.
    Terminal.WriteLine(13, ">> ");

    // Show cursor again.
    Terminal.ShowCursor();
  }

  // Main game loop.
  public override async Task GameLoop () {
    while (true) {
      // Update game state.
      UpdateLocalGameState();

      // Draw game board.
      DrawGame();

      // Is game over?
      if (CurrentGame.IsGameOver()) {
        Terminal.ClearLines(1, 2);
        Terminal.WriteLine(1, "Game over!");
        await Task.Delay(1000);
        if (CurrentGame.GetWinner() == 0) {
          Terminal.WriteLine(2, "Draw!");
        } else if (CurrentGame.GetWinner() == Player) {
          Terminal.WriteLine(2, "You won!");
        } else {
          Terminal.WriteLine(2, "You lost!");
        }
        await Task.Delay(5000);
        for (int i = 0; i <= 15; i++) {
          Terminal.ClearLine(i);
          await Task.Delay(100);
        }
        Terminal.SetCursor(0, 0);
        break;
      }

      // Is it our turn?
      if (CurrentGame.ToMove == Player) {
        // Wait for human player to make a move.
        string? input = Console.ReadLine();
        // Parse input to a number between 1 and 7.
        int column = string.IsNullOrEmpty(input) ? 0 : int.Parse(input) - 1;
        if (column < 0 || column > 6) {
          continue;
        }

        // Make a move.
        try {
          UpdateRemoteGameState(column);
        } catch {
          // Illegal move.
          Terminal.HideCursor();
          Terminal.WriteLine(1, "Invalid move!");
          await Task.Delay(3000);
          Terminal.ClearLine(1);
        } finally {
          Terminal.ClearLine(13);
        }
        continue;
      } else {
        Terminal.HideCursor();
        // Poll server every second for move.
        // Spin spinner.
        Spinner spinner = new();
        for (int i = 0; i < spinner.Length(); i++) {
          Terminal.WriteLine(0, $" [ CONNECT FOUR ] {spinner.Get()}");
          await Task.Delay(1000 / spinner.Length());
        }
      }
      // Repeat loop.
    }
  }
}