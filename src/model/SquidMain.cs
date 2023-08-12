using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using OpenCvSharp;

namespace Squid;

// jsonをArgs等に使用する際、"のエスケープの為、二重にSerialize

public enum MessengerType {
  message,
  json
}

public struct Messenger {
  public string type { get; set;}
  public object payload { get; set;}

  public static Messenger CreateFromSting(string value){
    return new Messenger(){
      type = MessengerType.message.ToString(),
      payload = value
    };
  }

  public static Messenger CreateFromJson(string json){
    return System.Text.Json.JsonSerializer.Deserialize<Messenger>(json);
    // return new Messenger(){
    //   type = MessengerType.json,
    //   payload = System.Text.Json.JsonSerializer.Deserialize<object>(json)
    // };
  }

  public string ToJson(){
    var options = new JsonSerializerOptions();
    options.Converters.Add(new JsonStringEnumConverter());
    return System.Text.Json.JsonSerializer.Serialize(this, options);
  }

}

/*
  ExecuteScriptAsync : javascriptの呼び出し
  await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync :
    DOM が作成されると、すべてのページで実行される

  cs: public dynamic this[dynamic i] => 14 + i;
  js: console.log(await squid[4.4])
    => 144.4
  stringとして受けてる

  js to cs
  hoge("5") => System.String
  hoge(5) => System.Int32
  hoge(5.4) => System.Double
  hoge([4, 5.5 ,"a"]) => System.Object[] ([System.Int32, System.Double, System.String] )
  hoge(new Int32Array[4, 5 ,6]) => System.__ComObject

  option引数は、引数名を指定できない方式
  javascript側で対応するにはオブジェクトリテラルにする
*/

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class SquidView {

  protected internal Action<string> setTitleAct;
  protected internal Action<string> setStatAct;
  protected internal Microsoft.Web.WebView2.Wpf.WebView2 webView;

  protected internal MainWindow window;

  protected internal MemoryMap memoryMap;
  protected internal Pipe pipe = new Pipe();

  private SquidView() { }

  protected internal static async Task<SquidView> Build(MainWindow window, Microsoft.Web.WebView2.Wpf.WebView2 webView, string key, string comment = "SquidView.Build"){
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($$""" console.log('{{comment}}') """);
    var obj = new SquidView();
    obj.window = window;
    obj.webView = webView;
    webView.CoreWebView2.AddHostObjectToScript(key, obj);
    return obj;
  }

  /* background */
  // protected internal void RunPipe(MainWindow obj, string name){
  //   pipe = new Task(async () => {
  //     try{
  //       while (true) {
  //         using var stream = new NamedPipeServerStream(name);
  //         await stream.WaitForConnectionAsync();
  //         using var reader = new StreamReader(stream);
  //         var src = await reader.ReadToEndAsync();
  //         obj.Dispatcher.Invoke((Action)(() => {
  //           JsonSendToView(src.Trim());
  //         }));        
  //       }
  //     }catch(Exception e){
  //       obj.Dispatcher.Invoke((Action)(() => {
  //         this.MessageSendToView(e.ToString());
  //       }));      
  //     }
  //   });
  //   pipe.Start();
  // }

  public void OpenNamedPipe(string name) => pipe.Run(name,
    (src) => {
      window.Dispatcher.Invoke((Action)(() => {
        this.JsonSendToView(src);
      }));
    },
    (e) => {
      window.Dispatcher.Invoke((Action)(() => {
        this.MessageSendToView(e);
      }));
    }
  );
  /*
    pipe = new Task(async () =>
    {
      try{
        while (true) {
          using var stream = new NamedPipeServerStream(name);
          await stream.WaitForConnectionAsync();
          using var reader = new StreamReader(stream);
          var src = await reader.ReadToEndAsync();
          window.Dispatcher.Invoke((Action)(() => {
            JsonSendToView(src.Trim());
          }));        
        }
      }catch(Exception e){
        window.Dispatcher.Invoke((Action)(() => {
          this.MessageSendToView(e.ToString());
        }));      
      }
    });
    pipe.Start();
    */

  public async void CancelNamedPipe() {
    await pipe.Cancel();
    this.MessageSendToView("pipe canceled");
  }

  public void OpenMemoryMap(string name, int size, string type = null){
    memoryMap = type switch {
      "byte" => new MemoryMap<byte>(name, size),
      "int" => new MemoryMap<int>(name, size),
      "double" => new MemoryMap<double>(name, size),
      _=> new MemoryMap<int>(name, size)
    };
    // memoryMap = type switch {
    //   "byte" => new MemoryMap<byte>(name, Enumerable.Range(0, size).Select(n=>(byte)n).ToArray<byte>()),
    //   "int" => new MemoryMap<int>(name, Enumerable.Range(1, size+1).ToArray<int>()),
    //   "double" => new MemoryMap<double>(name, Enumerable.Range(2, size+2).Select(n=>(double)n).ToArray<double>()),
    //   _=> new MemoryMap<int>(name, Enumerable.Range(3, size+3).ToArray<int>())
    // };
  }

  public void CloseMemoryMap(){
    memoryMap.Close();
  }

  public void WriteMemoryMap(object[] obj, string type = null){
    dynamic src = type switch {
      "byte" => obj.Select(n => (byte)n).ToArray<byte>(),
      "int" => obj.Select(n => (int)n).ToArray<int>(),
      "double" => obj.Select(n => (double)n).ToArray<double>(),
      _=> obj.Select(n => (int)n).ToArray<int>()
    };
    // var src = JsonSerializer.Deserialize<int[]>(arrayJson);
    if(!(memoryMap is null)){
      memoryMap.Write(src);
    }
  }

  public dynamic ReadMemoryMap(){
    if(!(memoryMap is null)){
      return memoryMap.Read();
    }
    return null;
  }

  public dynamic ReadMemoryMapSingle(int index){
    if(!(memoryMap is null)){
      return memoryMap.Read(index);
    }
    return null;
  }

  /* Image */
  public ushort[] Demosaicing(int width, int height, string type, int bitshift = 0) {
    var src_uint16 = memoryMap.Read() switch {
      byte[] a => a.Select(n => (UInt16)n).ToArray<UInt16>(),
      int[] a => a.Select(m => {
        var n = bitshift >= 0 ? m >> bitshift : m << -1 * bitshift;
        return n > UInt16.MaxValue ? (UInt16)UInt16.MaxValue : n < 0 ? (UInt16)0 : (UInt16)n;
      }).ToArray<UInt16>(),
      double[] a => a.Select(n => n > UInt16.MaxValue ? (UInt16)UInt16.MaxValue : n < 0 ? (UInt16)0 : (UInt16)n).ToArray<UInt16>(),
      _=> throw new Exception() 
    };

    var dst_bgr = new ushort[width * height * 3];
    var dst_bgra = new ushort[width * height * 4];
    var alpha = Enumerable.Repeat<ushort>(UInt16.MaxValue, width * height).ToArray();

    using(var mat_src = new Mat(height, width, MatType.CV_16UC1, src_uint16)) 
    using(var mat_bgr = new Mat(height, width, MatType.CV_16UC3, dst_bgr))
    using(var mat_alpha = new Mat(height, width, MatType.CV_16UC1, alpha))
    using(var mat_bgra = new Mat(height, width, MatType.CV_16UC4, dst_bgra)) {
      switch(type) {
        case "bayerRG" : Cv2.Demosaicing(mat_src, mat_bgr, ColorConversionCodes.BayerRG2BGR); break;
        case "bayerGR" : Cv2.Demosaicing(mat_src, mat_bgr, ColorConversionCodes.BayerGR2BGR); break;
        case "bayerBG" : Cv2.Demosaicing(mat_src, mat_bgr, ColorConversionCodes.BayerBG2BGR); break;
        case "bayerGB" : Cv2.Demosaicing(mat_src, mat_bgr, ColorConversionCodes.BayerGB2BGR); break;
        case "mono" : Cv2.Merge(new Mat[]{mat_src, mat_src, mat_src}, mat_bgr); break;
        default: Cv2.Merge(new Mat[]{mat_src, mat_src, mat_src}, mat_bgr); break;
      }
      Cv2.Merge(new Mat[]{mat_bgr, mat_alpha}, mat_bgra);
      return dst_bgra;
    }
  }


  /* To View */

  protected internal void MessageSendToView(string val) {
    var json = Messenger.CreateFromSting(val).ToJson();
    webView.CoreWebView2.PostWebMessageAsJson(json);
    // PostWebMessageAsJson   -> window.chrome.webview.addEventListener('message', (e) => { let json = e.data
    // PostWebMessageAsString -> window.chrome.webview.addEventListener('message', (e) => { let json = JSON.parse(e.data);
  }

  protected internal void JsonSendToView(string val) {
    var json = Messenger.CreateFromJson(val).ToJson();
    webView.CoreWebView2.PostWebMessageAsJson(json);
  }


  /* From View */

  // private void MainWindow.MessageReceived

  public void SetTitle(string src) {
    setTitleAct(src switch {
      null => $"Squid - {webView.CoreWebView2.DocumentTitle}",
      _=> $"Squid - {src}"
    });
  }

  public void SetStatus(string src) {
    setStatAct(src);
  }

  public void ShowMessageBoxLite(string s) {
    MessageSendToView("show messagebox");
    MessageBox.Show(s);
    MessageSendToView("closed messagebox");
  }

  public int ShowMessageBox(string text, string caption, string button, string ico, string def) {
    return (int)MessageBox.Show(
      text, caption,
      Enum.Parse<MessageBoxButton>(button),
      Enum.Parse<MessageBoxImage>(ico),
      Enum.Parse<MessageBoxResult>(def)
    );
  }

  public void Navigate(string url) {
    MessageSendToView($"{url}");
    if (webView != null && webView.CoreWebView2 != null) {
      if (Uri.IsWellFormedUriString(url, UriKind.Absolute)){
        webView.CoreWebView2.Navigate(url);
      }else{
        webView.CoreWebView2.Navigate(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), url));
      }
    }
  }

  public void OpenDevTools() {
    webView.CoreWebView2.OpenDevToolsWindow();
  }
  
  public void SetWindowState(int? left, int? top, int? width, int? height) {
    if(width is not null) Application.Current.MainWindow.Width = (double)width;
    if(height is not null) Application.Current.MainWindow.Height = (double)height;
    if(left is not null) Application.Current.MainWindow.Left = (double)left;
    if(top is not null) Application.Current.MainWindow.Top = (double)top;
  }



  Dictionary<string, string> id = new Dictionary<string, string>();
  public string GetScriptID(string key) {
    return id.ContainsKey(key) switch {
      true => id[key],
      false => null,
    };
  }
  public async Task<string> AddScript(string key, string code) {
    if(id.ContainsKey(key)) { 
      webView.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(id[key]);
      id.Remove(key);
    }
    var result = await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(code);
    id.Add(key, result);
    return result;
  }
  public void RemoveScript(string key) {
    if(id.ContainsKey(key)){
      webView.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(id[key]);
      id.Remove(key);
    }
  }

  public async Task<string> AddJS2(string test) {
    var result = await webView.CoreWebView2.ExecuteScriptAsync(test);
    return result;
  }
  
  
  /* call process */
  // 戻り値を参照する場合は非同期(async/await)で実行


  public async Task<string> _CallProcessAsync(string com, string arg, bool window = false) {
    this.MessageSendToView("CallProcessAsync");
    var pi = new ProcessStartInfo() {
      FileName = com, //"dotnet",
      Arguments = arg, //"--info",
      RedirectStandardInput = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = !window
    };

    try { using (var proc = new Process()) {
      proc.EnableRaisingEvents = true;
      proc.StartInfo = pi;
      proc.Start();
      var results = proc.StandardOutput.ReadToEnd();
      var err = proc.StandardError.ReadToEnd();
      await proc.WaitForExitAsync();
      webView.CoreWebView2.PostWebMessageAsString($"{results}{err}");
    }}catch(Exception e){
      webView.CoreWebView2.PostWebMessageAsString($"{e}");
    }
    return "end";
  }


  public async void CallProcessAsync(string com, string arg, bool window = false, string callback = null) {
    this.MessageSendToView("CallProcessAsync");
    var pi = new ProcessStartInfo() {
      FileName = com, //"dotnet",
      Arguments = arg, //"--info",
      RedirectStandardInput = false,
      RedirectStandardOutput = !window,
      RedirectStandardError = !window,
      UseShellExecute = false,
      CreateNoWindow = !window
    };

    using (var proc = new Process()) {
      proc.EnableRaisingEvents = true;
      proc.StartInfo = pi;
      await Task.Run(() =>{
        try{
          proc.Start();
          var results = window ? null : proc.StandardOutput.ReadToEnd();
          var err = window ? null :proc.StandardError.ReadToEnd();
          proc.WaitForExit();

          webView.Dispatcher.Invoke((Action)(() => {
            // webView.CoreWebView2.PostWebMessageAsString($"console out : {results}{err}");
            this.MessageSendToView($"console out : {results}{err}");
          }));
        }catch(Exception e){
          webView.Dispatcher.Invoke((Action)(() => {
            // webView.CoreWebView2.PostWebMessageAsString();
            this.MessageSendToView($"err : {e}");
          }));
        }
      }).ContinueWith((e)=> {
        // webView呼べない（落ちる
        webView.Dispatcher.Invoke((Action)(() =>
        {
          if(callback is not null) webView.CoreWebView2.ExecuteScriptAsync(callback);
          // webView.CoreWebView2.ExecuteScriptAsync("document.querySelector(\"body\").style.backgroundColor=\"red\"").ConfigureAwait(false);
        }));
      });
    }
  }


  public string CallProcess(string com, string arg, bool window = true) {
    MessageSendToView("CallProcess");
    var pi = new ProcessStartInfo() {
      FileName = com, //"dotnet",
      Arguments = arg, //"--info",
      RedirectStandardOutput = !window,
      RedirectStandardError = !window,
      UseShellExecute = false,
      CreateNoWindow = !window
    };

    using (var proc = new Process()) {
      proc.EnableRaisingEvents = true;
      proc.StartInfo = pi;
      proc.Start(); 
      proc.WaitForExit();
      return "exit";
    }
  }
  

  public void Test(string a = "a", string b = "b", dynamic obj = null) {
    this.MessageSendToView($"Test : {a} {b} {obj}");
    // if(obj is not null) {
    try{
      foreach(object i in obj as object[]){
        Action f = i switch {
          int n =>()=> this.MessageSendToView($"int : {n}"),
          double n =>()=> this.MessageSendToView($"double : {n}"),
          string n =>()=> this.MessageSendToView($"string : {n}"),
          object n =>()=> this.MessageSendToView($"obj : {n}"),
          _=>()=> this.MessageSendToView($"other : {i}"),
        };
        f();
      }
    }catch(Exception e){
      this.MessageSendToView($"err : {e}");
    }

  }


  public void Sample(dynamic test){
    Type variableType = test.GetType();
    this.MessageSendToView($"{variableType}");
    foreach(var i in test) {
      Type v = i.GetType();
      this.MessageSendToView($"{v} {i}");
    }

    if(memoryMap is null) return;
    
    var a = memoryMap.Read();
    var dst = new UInt16[10];
    for(var i=0; i<5; i++){
      dst[i] = (UInt16)a[i];
    }
    // using(var mat = new OpenCvSharp.Mat(5, 1, OpenCvSharp.MatType.CV_16UC1, dst)) {
    //   OpenCvSharp.Cv2.ImShow("src", mat);
    //   OpenCvSharp.Cv2.WaitKey(0);  
    //   OpenCvSharp.Cv2.DestroyAllWindows();
    // }
    return;
  }


}