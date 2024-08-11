using System.Data;

namespace connect_four;

// Bot client for a bot player.
public class BotClient : Client {

  private readonly IBot bot;
  public BotClient () {
    bot = new C4bot();
    Console.Clear();
    Console.WriteLine($"Starting {bot.Name}...");
  }
  
  public override async Task GameLoop () {
    while (!CurrentGame.IsGameOver()) {
      // Update game state.
      UpdateLocalGameState();

      // Wait for our move.
      if (CurrentGame.ToMove == Player) {
        Console.WriteLine("Move has started.");
        // Make a move.
        int column = await bot.GetMove(CurrentGame);
        Console.WriteLine($"Move is {column + 1}.");
        try {
          UpdateRemoteGameState(column);
        } catch (Exception e) {
          Console.WriteLine($"Invalid move: {e.Message}");
          continue;
        }
      }
      // Poll server every second.
      await Task.Delay(1000);
    }
  }
}

// Interface for bots.
interface IBot {
  Task<int> GetMove(Game game);
  public string Name { get; }
}