// Terminal utilities.

namespace connect_four;
public static class Terminal {

  public static void Clear() {
    Console.Clear();
  }

  public static void ClearLine(int line) {
    Console.SetCursorPosition(0, line);
    Console.Write(new string(' ', Console.WindowWidth));
  }

  public static void ClearLines(int startLine, int endLine) {
    for (int line = startLine; line <= endLine; line++) {
      ClearLine(line);
    }
  }

  public static void WriteLine(int line, string text) {
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.SetCursorPosition(0, line);
    Console.Write(text);
  }

  public static void WriteChar(int line, int column, char ch) {
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.SetCursorPosition(column, line);
    Console.Write(ch);
  }

  public static void HideCursor() {
    Console.CursorVisible = false;
  }

  public static void ShowCursor() {
    Console.CursorVisible = true;
  }

  public static void SetCursor (int line, int column) {
    Console.SetCursorPosition(column, line);
  }
}

class Spinner {
  // private readonly string[] spinnerStates = new string[] { "⠁", "⠂", "⠄", "⡀", "⢀", "⠠", "⠐", "⠈" };
  private readonly string[] spinnerStates = new string[] { "⣷", "⣯", "⣟", "⡿", "⢿", "⣻", "⣽", "⣾" };
  private int spinnerIndex = 0;

  public int Length() {
    return spinnerStates.Length;
  }

  public string Get() {
    spinnerIndex = (spinnerIndex + 1) % spinnerStates.Length;
    return spinnerStates[spinnerIndex];
  }
}