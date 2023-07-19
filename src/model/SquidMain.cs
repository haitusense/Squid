using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.IO.Pipes;

namespace Squid;

// jsonをArgs等に使用する際、"のエスケープの為、二重にSerialize

public enum MessengerType {
  message,
  json
}

public struct Messenger {
  public MessengerType key { get; set;}
  public object value { get; set;}

  public static Messenger CreateFromSting(string value){
    return new Messenger(){
      key = MessengerType.message,
      value = value
    };
  }

  public static Messenger CreateFromJson(string json){
    return new Messenger(){
      key = MessengerType.json,
      value = JsonSerializer.Deserialize<object>(json)
    };
  }

  public string ToJson(){
    var options = new JsonSerializerOptions();
    options.Converters.Add(new JsonStringEnumConverter());
    return JsonSerializer.Serialize(this, options);
  }
}

/*
  ExecuteScriptAsync : javascriptの呼び出し
  await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync :
    DOM が作成されると、すべてのページで実行される
*/

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class SquidView
{
  protected internal Action<string> setTitleAct;
  protected internal Action<string> setStatAct;
  protected internal Microsoft.Web.WebView2.Wpf.WebView2 webView;

  protected internal MemoryMap<int> memoryMap;
  protected internal Task pipe;

  private SquidView(){ }

  protected internal static SquidView Build(Microsoft.Web.WebView2.Wpf.WebView2 webView, string key){
    var obj = new SquidView();
    obj.webView = webView;
    webView.CoreWebView2.AddHostObjectToScript(key, obj);

    // javascript コードの登録
    webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" console.log("add js code from cs") """);

    return obj;
  }

  /* background */
  protected internal void RunPipe(MainWindow obj, string name){
    pipe = new Task(async () =>
    {
      try{
        while (true) {
          using var stream = new NamedPipeServerStream(name);
          await stream.WaitForConnectionAsync();
          using var reader = new StreamReader(stream);
          var src = await reader.ReadToEndAsync();
          obj.Dispatcher.Invoke((Action)(() => {
            JsonSendToView(src.Trim());
          }));        
        }
      }catch(Exception e){
        obj.Dispatcher.Invoke((Action)(() => {
          this.MessageSendToView(e.ToString());
        }));      
      }
    });
    pipe.Start();
  }

  public void OpenMemoryMap(string name, int size){
    memoryMap = new MemoryMap<int>(name, Enumerable.Range(0, size).ToArray<int>());
  }

  public void CloseMemoryMap(){
    memoryMap.Close();
  }

  public void WriteMemoryMap(string arrayJson){
    var src = JsonSerializer.Deserialize<int[]>(arrayJson)!;
    if(!(memoryMap is null)){
      memoryMap.Write(src);
    }
  }

  public int[] ReadMemoryMap(){
    if(!(memoryMap is null)){
      return memoryMap.Read();
    }
    return null;
  }


  /* To View */

  protected internal void MessageSendToView(string val){
    var json = Messenger.CreateFromSting(val).ToJson();
    webView.CoreWebView2.PostWebMessageAsJson(json);
    // PostWebMessageAsJson   -> window.chrome.webview.addEventListener('message', (e) => { let json = e.data
    // PostWebMessageAsString -> window.chrome.webview.addEventListener('message', (e) => { let json = JSON.parse(e.data);
  }

  protected internal void JsonSendToView(string val){
    var json = Messenger.CreateFromJson(val).ToJson();
    webView.CoreWebView2.PostWebMessageAsJson(json);
  }


  /* From View */

  // private void MainWindow.MessageReceived

  public void SetTitle(string src){
    setTitleAct(src switch {
      null => $"Squid - {webView.CoreWebView2.DocumentTitle}",
      _=> $"Squid - {src}"
    });
  }

  public void SetStatus(string src){
    setStatAct(src);
  }

  public void ShowMessageBox(string s)
  {
    MessageSendToView("show messagebox");
    MessageBox.Show(s);
    MessageSendToView("closed messagebox");
  }

  public void Navigate(string s)
  {
    if (webView != null && webView.CoreWebView2 != null)
    {
      webView.CoreWebView2.Navigate(s);
    }
  }

  public void OpenDevTools()
  {
    webView.CoreWebView2.OpenDevToolsWindow();
  }
  

  /* call process */
  // 戻り値を参照する場合は非同期(async/await)で実行

  public async void CallProcessAsync(string com, string arg, string callback)
  {
    this.MessageSendToView("CallProcessAsync");
    bool window = true;
    var pi = new ProcessStartInfo()
    {
      FileName = com, //"dotnet",
      Arguments = arg, //"--info",
      RedirectStandardInput = false,
      RedirectStandardOutput = false,
      RedirectStandardError = false,
      UseShellExecute = false,
      CreateNoWindow = !window
    };

    using (var proc = new Process()) {
      proc.EnableRaisingEvents = true;
      proc.StartInfo = pi;
      await Task.Run(() =>{
        proc.Start(); 
        proc.WaitForExit();
        // webView.CoreWebView2.ExecuteScriptAsync("document.querySelector(\"body\").style.backgroundColor=\"red\"").ConfigureAwait(false);
        // webView.CoreWebView2.PostWebMessageAsString("CallProcessAsync");
        
      }).ContinueWith((e)=> {
        // webView呼べない（落ちる
        webView.Dispatcher.Invoke((Action)(() =>
        {
          webView.CoreWebView2.ExecuteScriptAsync(callback);
          // webView.CoreWebView2.ExecuteScriptAsync("document.querySelector(\"body\").style.backgroundColor=\"red\"").ConfigureAwait(false);
          // webView.CoreWebView2.PostWebMessageAsString("CallProcessAsync2");
        }));
      });
    }
    
    
    // Task<string>で返すとJS側で待ってくれない
    // var r = await webView.CoreWebView2.ExecuteScriptAsync("exit");
    //    string results = p.StandardOutput.ReadToEnd();
    //    string err = p.StandardError.ReadToEnd();
    //    p.WaitForExit();
    //    p.Close();
    //    return results;

  }

  public string CallProcess(string com, string arg, bool window = true)
  {
    MessageSendToView("CallProcess");
    var pi = new ProcessStartInfo()
    {
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
  
}
