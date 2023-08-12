using System.Threading.Tasks;

namespace Squid;

public static class RegistJavascript {

  public static string consoleColor = """
    Function.prototype.ex_method = function (name, func) {
      this.prototype[name] = func;
      return this;
    };
    String.ex_method('red',    function () { return `\u001b[31m${this}\u001b[0m`; });
    String.ex_method('green',  function () { return `\u001b[32m${this}\u001b[0m`; });
    String.ex_method('yellow', function () { return `\u001b[33m${this}\u001b[0m`; });
    String.ex_method('blue',   function () { return `\u001b[34m${this}\u001b[0m`; });
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
    SquidJS.drawFromMemoryMap = async function(id, color, bitshift) {
      const canvas = document.getElementById(id);
      const ctx = canvas.getContext('2d', { willReadFrequently: true, alpha: false });
      const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);

      const dst = await this.hostObj.Demosaicing(canvas.width, canvas.height, color, bitshift);
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


  public static string keyEvent = """
    SquidJS.addKeyMouseEvent = function(elem, obj, act) {

      elem.addEventListener("mouseenter", async (event) => { await act(event, 'MouseEnter'); });
      
      elem.addEventListener("mouseleave", async (event) => { await act(event, 'MouseLeave'); }); 

      elem.addEventListener('keydown', async (event) => {
        if(event.repeat) return;
        let dst = `${event.shiftKey ? 'Shift+' : ''}${event.ctrlKey ? 'Ctrl+' : ''}${event.altKey ? 'Alt+' : ''}${event.code ?? ''}`;
        await act(event, dst);
      });

      elem.addEventListener('mousewheel', async (event) => {
        let key = `${event.shiftKey ? 'Shift+' : ''}${event.ctrlKey ? 'Ctrl+' : ''}${event.altKey ? 'Alt+' : ''}`;
        let wheel = event.wheelDelta > 0 ? "WheelUP" : "WheelDOWN";
        await act(event, `${key}${wheel}`)
      });

      elem.addEventListener('mousemove', async (event) => { await act(event, "MouseMove"); });

    }
  """;



  public static string main(string val) => $$"""
    const SquidJS = {
      hostObj : chrome.webview.hostObjects.Squid,
      args : function() { return JSON.parse('{{val}}'); }
    };

    /* 
      allow演算子にするとthisが使えない
      (クロージャーなのでobjectじゃなくてwindowの方をキャプチャする)
    */
    SquidJS.setWindowState = function(obj) {
      this.hostObj.SetWindowState(
        obj.left,
        obj.top,
        obj.width,
        obj.height
      );
    }

  """;

  public static string stateManagement = $$"""

    SquidJS.useDispatch = async function(action) {
      SquidJS._state = await SquidJS._store(SquidJS._state, action);
      SquidJS.useSelector(SquidJS._state);
    }

    SquidJS.createAsyncDispatch = function(initialState, reducer) {
      window.chrome.webview.addEventListener('message', async (e) => {
        let action = e.data;
        SquidJS._state = await SquidJS._store(SquidJS._state, action);
        SquidJS.useSelector(SquidJS._state);
      });
    }

    SquidJS.createStore = async function(initialState, reducer) {
      var newID1 = await this.hostObj.AddScript("store", `SquidJS._store = ${''+reducer};`);
      var newID2 = await this.hostObj.AddScript("initialState", `SquidJS._state = ${JSON.stringify(initialState)};`);
      return `${newID1} ${newID2}`
    }

    SquidJS.useSelector = async function(state) { }

    SquidJS.callProcess = async function(obj) {
      var cb = '' + obj.callback;
      var result = await this.hostObj.CallProcessAsync(obj.com, obj.args, obj.window, `(${cb})()`);
      return true;
    }
    



  """;
    
  /*
    // action()
    SquidJS.createAction = function(action) {
      this.Action = action
    }

    const count = useSelector((state) => state.count)
    const dispatch = useDispatch();
    const increment = () => { dispatch({ type: 'multi', payload: 2 }); }
  */

  public static async Task<string> AddJavascriptAsync(this Microsoft.Web.WebView2.Wpf.WebView2 webView, string args, string comment = "AddJavascriptAsync") {

    /* DOM が作成されると、すべてのページで実行される */
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($$""" console.log('{{comment}}') """);
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.main(args));
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.consoleColor);
    // await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.setMessageEventListener);
    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.draw);

    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.keyEvent);

    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.stateManagement);
    
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