using System.Collections;
using System.Net.NetworkInformation;
namespace connect_four;

// A bot implementation using a tree structure search.
public class C4bot : IBot {

  public string Name => "C4bot";
  private readonly int searchDepth = 7;
  // Hash map to store known states. Key is hash of state and value is Node.
  private static Hashtable knownStates = new();

  private static Random rng = new();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<int> GetMove(Game game) {
    // Search the game tree for the best move.
    Console.WriteLine("C4 > Finding best move...");

    // Start searching.
    // Run MoveFinder on other thread and then return result.
    Task<(int, int)> finder = Task.Run(async () => await MoveFinder(new Node(game, null, null), searchDepth));
    // Return first item of tuple.
    return (await finder).Item1;
  }

  // Returns: (column, advantage)
  private static async Task<(int, int)> MoveFinder(Node startNode, int depth = 50) {
    int posCount = 0;
    try {
      Console.WriteLine($"C4 > Starting move finder with depth {depth}...");
      // Double check game is not over.
      if (startNode.state.IsGameOver()) {
        // Return random move as we should not end up in this situation.
        return (0, 0);
      }
      // Chain of nodes that leads to the current node that is being explored.
      List<Node> stack = new() { startNode };
      // Use a loop to traverse the tree to avoid a stack overflow.
      // C# does not lend itself to tail recursion.
      while (true) {
        // Get current node.
        Node node = stack.Last();

        // Hash representing state of the board used for caching.
        int stateHash = node.state.Board.gameBoard.GetHashCode();

        // Have we explored this node before?
        if (knownStates.ContainsKey(stateHash) && stack.Count > 1) {
          node.advantage = ((Node)knownStates[stateHash]!).advantage!;
          node.distance = ((Node)knownStates[stateHash]!).distance!;
          stack.RemoveAt(stack.Count - 1);
          continue;
        }

        // Check if we are at the maximum depth.
        if (node.depth > depth && stack.Count > 1) {
          // Guess this node as a very distant draw.
          node.advantage = 0;
          node.distance = depth;
          // Check if this node is the root node.
          if (stack.Count == 1) {
            break;
          }
          // Done with this node.
          stack.RemoveAt(stack.Count - 1);
          continue;
        }

        // Is this node a leaf (game over / draw)?
        if (node.state.IsGameOver()) {
          posCount++;
          // Record winner.
          node.advantage = node.state.GetWinner();
          // Set distance to zero.
          node.distance = 0;

          string pathString = "";
          for (int i = 1; i < stack.Count; i++) {
            pathString += $"P{stack[i - 1].state.ToMove} -> {stack[i].move + 1} ";
          }
          Console.WriteLine($"C4 > End state {node.advantage} found after {node.depth} moves. Path: {pathString}");
          // Done with this node.
          stack.RemoveAt(stack.Count - 1);
          continue;
        } else {
          // Map all child nodes.
          // Node must have children because it is not a game-over state.
          if (node.children.Count < 1) {
            // Find all children. 
            node.children.AddRange(GetChildStates(node));
          }
          // Use a depth-first search by moving down to first child that hasn't been explored.
          Node? unexplored = node.children.Find(child => child.advantage == null);
          if (unexplored != null) {
            stack.Add(unexplored);
            continue;
          }

          // Not a leaf.
          // Determine advantage.
          node.advantage = FindAdvantage(node);
          // Determine distance.
          node.distance = FindDistance(node);
          // Advantage determined, move to parent if exists.
          if (stack.Count > 1) {
            posCount++;
            // Cache result.
            knownStates.Add(stateHash, node);
            stack.RemoveAt(stack.Count - 1);
            continue;
          } else {
            // This was the root node and we are done.
            break;
          }
        }
      }
      Console.WriteLine($"C4 > Move finder done. {posCount} positions explored.");
      if (stack.Count > 1) {
        Console.WriteLine("C4 > Stack has more than one element. This should not happen.");
      }
      Node startState = stack.Last();
      List<Node> children = startState.children!;
      
      string stateDescription = "it's a draw/undecided";
      bool losing = false;
      if (startState.advantage == startState.state.ToMove) {
        stateDescription = "I'm winning";        
      } else if (startState.advantage != 0) {
        stateDescription = "I'm losing";
        losing = true;
      }
      Console.WriteLine($"C4 > Current advantage is: {startState.advantage}. (I think {stateDescription})");

      string advantages = "";
      foreach (Node child in children!) {
        advantages += $"[{child.move + 1} -> {child.advantage} in {child.distance}] ";
      }
      Console.WriteLine($"C4 > Advantages for each move: {advantages}");

      // Return one of the moves that will lead to the advantage of the current player in the fewest moves.
      // When in a losing position, choose the one with the maximum distance and hope for a blunder.
      // If there are no children, return this state.
      // If there are multiple choices, pick at random (to avoid playing first column only when no winning path has been found).
      int targetDistance = 0;
      if (losing) {
        targetDistance = (int)children.FindAll(child => child.advantage == startState.advantage).Max(child => child.distance)!;
      } else {
        targetDistance = (int)children.FindAll(child => child.advantage == startState.advantage).Min(child => child.distance)!;
      }

      List<Node> candidates = children.FindAll(child => child.advantage == startState.advantage && child.distance == targetDistance);
      Console.WriteLine($"C4 > Found {candidates.Count} candidate moves.");

      Node bestMove = children.Count > 0 ? candidates[rng.Next(0, candidates.Count)]! : startState;
      Console.WriteLine($"C4 > Chose move {bestMove.move + 1} with advantage {bestMove.advantage}");

      return ((int)bestMove.move!, (int)bestMove.advantage!);
    } catch (Exception e) {
      Console.WriteLine(e);
      return (0, 0);
    }
    
  }

  // Finds the distance to a winning state.
  private static int FindDistance (Node node) {
    // From the children of this node, find the lowest distance value.
    if (node.children == null) {
      return 0;
    }
    return (int)node.children.Min(child => child.distance)! + 1;
  }

  // Finds the advantage for a node whose every child has been explored.
  private static int FindAdvantage(Node node) {
    // If the current player can move to a winning state, they will.
    Node? win = node.children!.Find(child => child.advantage == node.state.ToMove);
    if (win != null) {
      return (int)win.advantage!;
    }

    // If the player cannot move to a winning state, but they can move to a draw state, they will.
    Node? draw = node.children!.Find(child => child.advantage == 0);
    if (draw != null) {
      return 0;
    }

    // The player cannot move intoa draw or winning state.
    // Return 1 if the player to move is 2, and 2 if the player to move is 1.
    return node.state.ToMove == 1 ? 2 : 1;
  }

  private static List<Node> GetChildStates (Node node) {
    // Find all children.
    List<Node> children = new();
    for (int col = 0; col < 7; col++) {
      try {
        node.state.CheckLegalMove(node.state.ToMove, col);
        // Move is legal. Append child node.
        Game newState = (Game)node.state.Clone();
        newState.MakeMove(node.state.ToMove, col);
        Node child = new(newState, node, col);
        children.Add(child);
      } catch { }
    }
    return children;
  }

  private static void PrintState (Node node) {
    for (int i = node.state.Board.gameBoard.GetLength(0) - 1; i >= 0; i--)
      {
          for (int j = 0; j < node.state.Board.gameBoard.GetLength(1); j++)
          {
              Console.Write(node.state.Board.gameBoard[i, j] + " ");
          }
          Console.WriteLine();
      }
  }
}

class Node {
  // Game state.
  public Game state;
  // Parent game state that can lead to this game state.
  public Node? parent;
  // Children states that this state can lead to.
  public List<Node> children = new();
  // Which player is winning in this state.
  public int? advantage = null;
  // Depth of the node in the tree.
  public int depth;
  // Which move led to this state. Null if root.
  public int? move;
  // How many moves away from an end state this node is.
  public int? distance;

  public Node(Game state, Node? parent, int? move) {
    this.state = state;
    this.parent = parent;
    this.depth = parent == null ? 0 : parent.depth + 1;
    this.move = move;
  }
}