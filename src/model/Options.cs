using CommandLine;

namespace Squid;

class Options {

  [Option('d', "devtool", Required = false, Default = false)]
  public bool devtool { get; set; }

  [Option('s', "starturl", Required = false, Default = @"index.html")]
  public string starturl { get; set; } 

  [Option('w', "working-directory", Required = false)]
  public string working { get; set; } = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "");

  [Option('o', "hostobjects-name", Required = false, Default = "Squid")]
  public string hostobjects { get; set; }

  // urlエンコードしてのurlパラメータ渡しはあまりキレイでないので
  [Option('a', "args", Required = false, Default = "{}")]
  public string args { get; set; }

}

/*

public class Options {
  
  [Option('r', "received", Required = false, Default = @"main.csx")]
  public string received { get; set; } 

  [Option('p', "pipe", Required = false, Default = null)]
  public string pipe { get; set; }

  [Value(0, MetaName = "JSON", Required = false, Default = null)]
  public string json { get; set; }

  public static T Serializer<T>(IList<string> args) where T : Options {
    T dst = null;
    Parser.Default.ParseArguments<T>(args).WithParsed<T>(op => {
      if(op.json is not null){
        var buf = JsonSerializer.Deserialize<string>($""" "{op.json}" """);
        dst = JsonSerializer.Deserialize<T>(buf);
        var properties = typeof(T).GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
        foreach (var prop in properties)
        {
          var value = prop.GetValue(dst, null);
          if (value == null) prop.SetValue(dst, prop.GetValue(op, null), null);
        }
      }else{
        dst = op;
      }
    });
    return dst;
  }

}


*/