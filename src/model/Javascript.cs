using System.Threading.Tasks;

namespace Squid;

public static class RegistJavascript {

  public static string consoleColor = """
    class ConsoleColor {
      static color_red    = '\u001b[31m';
      static color_green  = '\u001b[32m';
      static color_yellow = '\u001b[33m';
      static color_blue   = '\u001b[34m';
      static color_reset  = '\u001b[0m';

      static red(src) { return this.color_red + src + this.color_reset }
      static green(src) { return this.color_green + src + this.color_reset }
      static yellow(src) { return this.color_yellow + src + this.color_reset }
      static blue(src) { return this.color_blue + src + this.color_reset }
    }
  """;

  public static string setMessageEventListener = """
    SquidJS.setMessageEventListener = (event) => {
      if(window.chrome.webview){
        window.chrome.webview.addEventListener('message', (e) => {
          var json = e.data;
          switch(json.key){
            case "message":
              console.log(`from cs ${json.key} : ${ConsoleColor.yellow(json.value)}`);
              break;
            case "json":
              console.log({ "from cs" : json.key, "value" : json.value});
              event(json.value);
              break;
            default:
              console.log(e.data);
          }
        });
      }
    }
  """;

  public static string draw = """
    SquidJS.drawFromMemoryMap = async (id, color, bitshift) => {
      const canvas = document.getElementById(id);
      const ctx = canvas.getContext('2d', { willReadFrequently: true, alpha: false });
      const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);

      const dst = await chrome.webview.hostObjects.Squid.Demosaicing(canvas.width, canvas.height, color, bitshift);
      const clamp = new Uint8ClampedArray(dst);
      // let hoge = dst[i] > 255 ? 255 : dst[i] < 0 ? 0 : src[i];   

      imageData.data.set(clamp);
      // let n = 0;
      // let data = imageData.data;
      // for (let i = 0; i < clamp.length; i+=4) {
      //   data[n++] = clamp[i];     // red
      //   data[n++] = clamp[i + 1]; // green
      //   data[n++] = clamp[i + 2]; // blue
      //   data[n++] = clamp[i + 3]; // a
      // }
      
      ctx.putImageData(imageData, 0, 0);
    }
  """;


  public static string main(string val) => $$"""
    SquidJS.args = () => JSON.parse('{{val}}');
    
    SquidJS.setWindowState = (obj) => {
      chrome.webview.hostObjects.Squid.SetWindowState(
        obj.left,
        obj.top,
        obj.width,
        obj.height
      )
    }


  """;

  public static async Task<string> AddJavascriptAsync(this Microsoft.Web.WebView2.Wpf.WebView2 webView, string args) {

    /* DOM が作成されると、すべてのページで実行される */
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" console.log("add js code from cs") """);
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" const SquidJS = {} """);
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.consoleColor);
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.setMessageEventListener);
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.draw);

    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.main(args));
  
    /*
      ExecuteScriptAsync DOMコンテンツが読み込まれたり、ナビゲーション完了後実行する 
      The injected scriptなので関数ではなく構文など与えると宣言で重複してエラーでる
    */

    return "";
  }

}

/*
スクリプトでホスト オブジェクトを使用する利点があるシナリオ
  キーボード API があり、Web 側から関数を keyboardObject.showKeyboard 呼び出す
  ファイル システムに直接アクセスなどサンドボックス化されてる
*/