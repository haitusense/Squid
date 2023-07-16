#r "nuget: Selenium.WebDriver, 4.10.0"
#r "nuget: WebDriverManager, 2.16.3"

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

var dst = Selenium.run("https://github.com/haitusense");
Console.WriteLine(dst);
WritePipe("NamedPipe", $$""" { "id" : "id7_out", "value" : "{{dst}}" } """);

public class Selenium {

  public static string run(string url) {
    new WebDriverManager.DriverManager().SetUpDriver(new WebDriverManager.DriverConfigs.Impl.ChromeConfig());
    ChromeOptions options = new ChromeOptions();
    var driver = new ChromeDriver(options);
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);

    driver.Navigate().GoToUrl(url);
    var dst = driver.FindElement(By.XPath("//*[@itemprop='additionalName']")).Text;

    driver.Quit();
    return dst;
  }
}

static void WritePipe(string key, string val){
  using (var pipeClient = new System.IO.Pipes.NamedPipeClientStream(key)) {
    pipeClient.Connect(100);
    using (var sw = new StreamWriter(pipeClient)) {    
      sw.WriteLine(val);
    }
  }
} 