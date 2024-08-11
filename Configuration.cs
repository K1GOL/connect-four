using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace connect_four;
public static class Configuration {
  public static readonly string version = $"{Assembly.GetExecutingAssembly().GetName().Version!.Major}.{Assembly.GetExecutingAssembly().GetName().Version!.Minor}.{Assembly.GetExecutingAssembly().GetName().Version!.Build} {RuntimeInformation.RuntimeIdentifier}";
  public static readonly IPAddress defaultIp = IPAddress.Parse("127.0.0.1");
  public static readonly IPAddress defaultServerIp = IPAddress.Any;
  public static readonly int defaultPort = 4444;
  public static readonly string[] symbols = { "O", "*" };
}