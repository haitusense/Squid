using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

try{
  // var node = JsonNode.Parse(Args[0]);
  for (int i = 0; i < 3; i++) {      
    Thread.Sleep(1000);
    WritePipe("NamedPipe", "progress", $$""" { "cnt" : {{i}} } """);
    Console.WriteLine(i);
  }
}catch(Exception e){
  Console.WriteLine(e.ToString());
  Console.ReadKey();
}

static void WritePipe(string key, string action, string payload){
  using (var pipeClient = new System.IO.Pipes.NamedPipeClientStream(key)) {
    pipeClient.Connect(100);
    using (var sw = new StreamWriter(pipeClient)) {
      var dst = $$""" { "type" : "{{action}}", "payload" : {{payload}} } """;
      // Console.WriteLine(dst);
      sw.WriteLine(dst);
    }
  }
}

static void WriteMMF(string key, int[] src){
  using(var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(key))
  using(var accessor = mmf.CreateViewAccessor())
  {
    accessor.Write(0, src.Length);
    accessor.WriteArray(sizeof(int), src, 0, src.Length);
  }
}
