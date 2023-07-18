#r "nuget: CommandLineParser, 2.4.3"

using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using System.Threading;
using System.IO.MemoryMappedFiles;

try {
  var op = JsonSerializer.Deserialize<Dictionary<string,string>>(Args[0]);
  Console.WriteLine($"key : {op["mmf"]}");

  var data = new int[640*480];
  for (int y = 0; y < 480; y++) {
    for (int x = 0; x < 640; x++) {
      data[x + y * 640] = y - 100;
    }
  }

  WriteMMF(op["mmf"], data);

}catch(Exception e){
  Console.WriteLine(e);
  Console.ReadKey();
}

static void WritePipe(string key, string val){
  using (var pipeClient = new System.IO.Pipes.NamedPipeClientStream(key)) {
    pipeClient.Connect(100);
    using (var sw = new StreamWriter(pipeClient)) {    
      sw.WriteLine(val);
    }
  }
} 

static void WriteMMF(string key, int[] src){
  using(var mmf = MemoryMappedFile.OpenExisting(key))
  using(var accessor = mmf.CreateViewAccessor())
  {
    accessor.Write(0, src.Length);
    accessor.WriteArray(sizeof(int), src, 0, src.Length);
  }
}
