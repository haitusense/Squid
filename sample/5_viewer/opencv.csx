// #r "nuget: OpenCvSharp4, 4.8.0.20230708"
// #r "nuget: OpenCvSharp4.runtime.win, 4.8.0.20230708"
#r "OpenCvSharp.dll"

using OpenCvSharp;

int width = 2672;
int height = 2064;
var src = File.ReadAllBytes("data.raw").Skip(64).ToArray();
var raw = Enumerable.Range(0, width * height).Select(x => x * 2)
          .Select(i => BitConverter.ToUInt16(new byte[] { src[i+1], src[i] }))
          .Select(i => i << 8)
          .ToArray();


using(var mat_src = new Mat(height, width, MatType.CV_16UC1, raw))
using(var mat_filter = new Mat(height, width, MatType.CV_16UC1))
using(var mat_dst = new Mat(height, width, MatType.CV_16UC1)) {
  
  // 長方形でMedianBlurしたいなら自分で実装
  // Cv2.MedianBlur(mat_src, mat_med, 5); 
  Cv2.GaussianBlur(mat_src, mat_filter, new Size(1, 9), 0);

  // 差分でWDとBDの区別できないのでAbsdiffでやるならoffset付けるなど工夫必要
  Cv2.Absdiff(mat_src, mat_filter, mat_dst);
  Cv2.Threshold(mat_dst, mat_dst, 30000, 65536, ThresholdTypes.Binary);

  Cv2.ImShow("src", mat_src);
  Cv2.ImShow("dst", mat_dst);
  Cv2.WaitKey(0);  
  Cv2.DestroyAllWindows();
  
  var dst = Cv2.CountNonZero(mat_dst);
  Console.WriteLine($"cnt : {dst}");
  WritePipe("NamedPipe", $$""" { "id" : "id8_out", "value" : "{{dst}}" } """);
}
        

static void WritePipe(string key, string val){
  using (var pipeClient = new System.IO.Pipes.NamedPipeClientStream(key)) {
    pipeClient.Connect(100);
    using (var sw = new StreamWriter(pipeClient)) {    
      sw.WriteLine(val);
    }
  }
} 