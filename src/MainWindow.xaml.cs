using System;
using System.Windows;
using CommandLine;
using Microsoft.Web.WebView2.Core;

namespace Squid;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
  Options op;
  SquidView squid;

  public MainWindow() {
    InitializeComponent();
    Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
  }

  private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
    MessageBox.Show(e.Exception.ToString(), "DispatcherUnhandledException");
    Environment.Exit(1);
  }

  private async void Window_Loaded(object sender, RoutedEventArgs e) {
    op = Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs()).MapResult(
      n => n,
      _ => throw new Exception("option err")
    );

    var webview_options = new CoreWebView2EnvironmentOptions("--allow-file-access-from-files");
    var environment = await CoreWebView2Environment.CreateAsync(null, null, webview_options);
    await webView.EnsureCoreWebView2Async(environment);
    // await webView.EnsureCoreWebView2Async(null);

    if(op.devtool) { webView.CoreWebView2.OpenDevToolsWindow(); }

    // await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" console.log("add event in cs") """);
    webView.NavigationCompleted += webView_NavigationCompleted;
    webView.CoreWebView2.WebMessageReceived += MessageReceived;

    /* jsで使用するクラスの登録 */
    squid = await SquidView.Build(this, webView, op.hostobjects, "loading : add objs form cs".Yellow());

    /* jsで使用するjs scriptの登録 */
    await webView.AddJavascriptAsync(op.args, "loading : add scripts form cs".Yellow());
    squid.setTitleAct = (n) =>{ this.Title = n; };
    // var label = this.FindName("statusLabel") as System.Windows.Controls.Label;
    // squid.setStatAct = (n) =>{ label.Content = n; };


    /* 画面遷移 */
    // await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" console.log("navigate in cs") """);
    var working = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), op.working);
    System.IO.Directory.SetCurrentDirectory(working);
    // squid.MessageSendToView(working);
    statusLabel.Text = working;
    // webView.CoreWebView2.Navigate(System.IO.Path.Combine(working, op.starturl));
    squid.Navigate(op.starturl);
    // squid.MessageSendToView("Window Loaded");
    // statusLabel.Text = "Window Loaded";
    // await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(""" console.log("navigated in cs") """);

    /* pipeの登録 */
    // if(!(op.pipe is null)) {
    //   squid.MessageSendToView("enable namedpipe");
    //   statusLabel.Text = "enable namedpipe";
    //   squid.RunPipe(this, op.pipe);
    // }

  }

  private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e) {
    squid.MessageSendToView("NavigationCompleted".Yellow());
    statusLabel.Text = "NavigationCompleted";
    squid.SetTitle(null);
  }

  private /*async*/ void MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args) {
    string text = args.TryGetWebMessageAsString();
    squid.MessageSendToView($"MessageReceived : {text}".Yellow());
    
    // Label.Contentだとアンダーバーがアクセスキーに使用されるので面倒
    statusLabel.Text = text; 

    /*
      // javascript
      var obj = {
        key: 'post',
        value: elem
      }
      var json = JSON.stringify(obj);
      window.chrome.webview.postMessage(elem);
    
      // MessageReceived
      var path = System.IO.Path.Combine(op.working, op.received);
      squid.MessageReceiveFromView(path, text);

    */
  }

  /*
    // evet
    private async void Button_Click(object sender, RoutedEventArgs e)
    {
      // call javascript function
      await webView.CoreWebView2.ExecuteScriptAsync($"Hoge()");

      //
      Debug.WriteLine(addressBar.Text);
      if (webView != null && webView.CoreWebView2 != null)
      {
        webView.CoreWebView2.Navigate(addressBar.Text);
      }
    }
  */


}

