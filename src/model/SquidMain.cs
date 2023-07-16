using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.IO.Pipes;

namespace Squid;

public struct Messenger{
  public string key{get; set;}
  public string value{get; set;}

  public static Messenger Create(string key, string value){
    return new Messenger(){
      key = key,
      value = value
    };
  }

  public string ToJson(){
    return JsonSerializer.Serialize(this);
  }
}

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class SquidView
{
  protected internal Action<string> setTitleAct;
  protected internal Action<string> setStatAct;
  protected internal Microsoft.Web.WebView2.Wpf.WebView2 webView;

  protected internal MemoryMap memoryMap;
  protected internal Task pipe;

  private SquidView(){}

  protected internal static SquidView Build(Microsoft.Web.WebView2.Wpf.WebView2 webView, string key){
    var obj = new SquidView();
    obj.webView = webView;
    webView.CoreWebView2.AddHostObjectToScript(key, obj);
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
            // this.MessageSendToView(json.Trim());
            var json = Messenger.Create("json", src.Trim()).ToJson();
            webView.CoreWebView2.PostWebMessageAsString(json);
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

  protected internal void RunMemoryMap(MainWindow obj, string name){
    memoryMap = new MemoryMap();
    memoryMap.Set(name, 10, 10, Enumerable.Range(0, 100).ToArray<int>());
  }



  /* To View */

  protected internal void MessageSendToView(string val){
    var json = Messenger.Create("message", val).ToJson();
    webView.CoreWebView2.PostWebMessageAsString(json);
  }

  // set js

  // await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener('message',  function(e) { alert('message received:' + e.data);});");
    

  /* From View */

  // public void MessageReceiveFromView(string path, string src){
  //   MessageSendToView("MessageReceiveFromView");
  //   try{
  //     var obj = JsonSerializer.Deserialize<Messenger>(src);
      
  //     switch(obj.key) {
  //       case "dotnet":
  //         Task.Run(() =>{
  //           // "のエスケープの為、二重にSerialize
  //           var json = JsonSerializer.Serialize(obj.value); 
  //           ProcessEx.CallProcessAsync("dotnet", $""" script "{path}" -- {json} """);
  //         }).ContinueWith((e)=> {
  //           // webView呼べない（落ちる
  //           webView.Dispatcher.Invoke((Action)(() =>
  //           {
  //             webView.CoreWebView2.PostWebMessageAsString("Task End");
  //           }));
  //         });
  //         break;
  //       case "status":
  //         setStatAct(obj.value);
  //         break;
  //       default:
  //         MessageSendToView("null obj");
  //         break;
  //     }
  //   }catch(Exception e){
  //     MessageSendToView(e.ToString());
  //   }


  //     // Task.Run(async () =>{
  //     //   using (var stream = new NamedPipeServerStream("testpipe")) {
  //     //     await stream.WaitForConnectionAsync();
  //     //     using (var reader = new StreamReader(stream)) {
  //     //       webView.Dispatcher.Invoke((Action)(() =>
  //     //       {
  //     //         webView.CoreWebView2.PostWebMessageAsString("Receive");
  //     //       }));
  //     //       var message = await reader.ReadLineAsync();
  //     //       webView.Dispatcher.Invoke((Action)(() =>
  //     //       {
  //     //         webView.CoreWebView2.PostWebMessageAsString($"Received: {message}");
  //     //       }));
  //     //     }
  //     //   }
  //     // });

  // }

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
  
  public async void CallProcessAsync(string com, string arg, string callback)
  {
    this.MessageSendToView("CallProcessAsync");
    bool window = true;
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

  //戻り値を参照する場合は非同期(async/await)で実行
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
  

  public void WriteMM(string arrayJson)
  {
    var src = JsonSerializer.Deserialize<int[]>(arrayJson)!;
    if(!(memoryMap is null)){
      memoryMap.Write(src);
    }
  }

  public int[] ReadMM()
  {
    if(!(memoryMap is null)){
      return memoryMap.Read();
    }
    return null;
  }

}

