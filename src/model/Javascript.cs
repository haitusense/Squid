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

  /*test*/
  // var a = new Uint8ClampedArray([-100,0,100,200,300])
  // -> 0 0 100 200 255

  public static string draw = """
    SquidJS.Draw = (id) => {
      var canvas = document.getElementById(id);
      var ctx = canvas.getContext('2d', { willReadFrequently: true });
      const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
      const data = imageData.data;

      for (var i = 0; i < data.length; i += 4) {
        data[i]     = 255; // red
        data[i + 1] = 0; // green
        data[i + 2] = 0; // blue
        data[i + 3] = 255; // a
      }

      ctx.putImageData(imageData, 0, 0);
    }

    SquidJS.DrawFromInt32 = (id, src) => {
      var canvas = document.getElementById(id);
      var ctx = canvas.getContext('2d', { willReadFrequently: true });
      const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
      const data = imageData.data;

      var hoge = new Uint8ClampedArray(src)
      // let hoge = src[i / 4] > 255 ? 255 : src[i / 4] < 0 ? 0 : src[i / 4]; 
      
      let j = 0;
      for (var i = 0; i < data.length / 4; i++) {
        data[j++] = hoge[i]; // red
        data[j++] = hoge[i]; // green
        data[j++] = hoge[i]; // blue
        data[j++] = 255;     // a
      }
      
      ctx.putImageData(imageData, 0, 0);
    }
  """;

  public static void Add(Microsoft.Web.WebView2.Wpf.WebView2 webView) {
    webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" console.log("add js code from cs") """);
    webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" const SquidJS = {} """);
    webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.consoleColor);
    webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RegistJavascript.draw);
  }

}