namespace Squid;

public static class ConsoleEx {
  const string color_red    = "\u001b[31m";
  const string color_green  = "\u001b[32m";
  const string color_yellow = "\u001b[33m";
  const string color_blue   = "\u001b[34m";
  const string color_reset  = "\u001b[0m";
  public static string Red(this string src) => $"{color_red}{src}{color_reset}";
  
  public static string Green(this string src) => $"{color_green}{src}{color_reset}";
  
  public static string Yellow(this string src) => $"{color_yellow}{src}{color_reset}";
  
  public static string Blue(this string src) => $"{color_blue}{src}{color_reset}";
  
}