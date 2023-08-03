#r "OpenCvSharp.dll"
using OpenCvSharp;
using System.Text.Json;
using System.Text.Json.Nodes;

try{
  var node = JsonNode.Parse(Args[0]);
  MainAction(node)();
}catch(Exception e){
  Console.WriteLine(e.ToString());
  Console.ReadKey();
}

static Action MainAction(JsonNode node){
  var flag = (string)node["flag"];
  var pipe = (string)node["pipe"];
  var mmf = (string)node["mmf"];

  Console.WriteLine(flag);
  Console.WriteLine(pipe);
  Console.WriteLine(mmf);

  Action dst = flag switch {
    "demosaic" => ()=> {
      using(var mat = new Mat("sample.jpg")) {
        var height = mat.Size(0);
        var width = mat.Size(1);
        using(var mat8 = new Mat(height, width, MatType.CV_8UC1)) 
        using(var mat8color = new Mat(height, width, MatType.CV_8UC3)) {

          for(var y=0; y<height; y++){
            for(var x=0; x<width; x++){
              var a = (x%2,y%2) switch {
                (0,1) => mat.At<Vec3b>(y, x)[0],
                (1,0) => mat.At<Vec3b>(y, x)[2],
                (_,_) => mat.At<Vec3b>(y, x)[1]
              };
              mat8.Set(y, x, a);
            }
          }

          // Cv2.Demosaicing(mat8, mat8color, ColorConversionCodes.BayerGB2BGR);
          var bytes = new byte[mat8.Total()];
          System.Runtime.InteropServices.Marshal.Copy(mat8.Data, bytes, 0, bytes.Length);
          WriteMMF(mmf, bytes.Select(n => (int)n).ToArray());
        
        }

      }
      WritePipe(pipe, $$""" { "status" : true } """);
    },
    _=> ()=> {
      WritePipe(pipe, $$""" { "status" : false } """);
    }
  };

  return dst;
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
  using(var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(key))
  using(var accessor = mmf.CreateViewAccessor())
  {
    accessor.Write(0, src.Length);
    accessor.WriteArray(sizeof(int), src, 0, src.Length);
  }
}
