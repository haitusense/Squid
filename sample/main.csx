#r "nuget: CommandLineParser, 2.4.3"

using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using System.Threading;

try{
  var op = Options.Serializer(Args);
  Console.WriteLine($"key : {op.key}");
  Console.WriteLine($"value : {op.value}");

  WritePipe("NamedPipe", $$""" { "id" : "id4_out", "value" : "start" } """);
  for (int i = 0; i < 5; i++)
  {      
    Thread.Sleep(1000);
    Console.WriteLine(i);
    WritePipe("NamedPipe", $$""" { "id" : "id4_out", "value" : "cnt {{i}}" } """);
  }

}catch(Exception e){
  Console.WriteLine(e);
  Console.ReadKey();
}
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Press any key to continue");
Console.ResetColor();
Console.ReadKey();

class Options
{
  public static Options Serializer(IList<string> args){
    Options dst = null;
    Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(op => {
      if(op.json is not null){
        var buf = JsonSerializer.Deserialize<string>($""" "{op.json}" """);
        dst = JsonSerializer.Deserialize<Options>(buf);
        var properties = typeof(Options).GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
        foreach (var prop in properties)
        {
          var value = prop.GetValue(dst, null);
          if (value == null) prop.SetValue(dst, prop.GetValue(op, null), null);
        }
      }else{
        dst = op;
      }
    });
    return dst;
  }

  [Value(0, MetaName = "JSON", Required = false, Default = null)]
  public string json { get; set; }

  [Option('k', "key", Required = false, Default = "default")]
  public string key { get; set; }

  [Option('v', "value", Required = false, Default = "default")]
  public string value { get; set; }

}

static void WritePipe(string key, string val){
  using (var pipeClient = new System.IO.Pipes.NamedPipeClientStream(key)) {
    pipeClient.Connect(100);
    using (var sw = new StreamWriter(pipeClient)) {    
      sw.WriteLine(val);
    }
  }
} 