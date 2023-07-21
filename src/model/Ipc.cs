using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;

namespace Squid;

public class MemoryMap<T> where T : struct {
  MemoryMappedFile mmf;

  public MemoryMap(string key, T[] src){
    mmf = MemoryMappedFile.CreateNew(key, sizeof(int) + src.Length * Marshal.SizeOf(typeof(T)));
    using(var accessor = mmf.CreateViewAccessor()){
      accessor.Write(0, src.Length);                        // 配列サイズの書き込み
      accessor.WriteArray(sizeof(int), src, 0, src.Length); // 本体の書き込み
      // int size = Marshal.SizeOf(src);
      // accessor.Write(sizeof(int), ref src, size);
    }
  }

  public int Length(){
    using(var accessor = mmf.CreateViewAccessor()){
      int size = accessor.ReadInt32(0);
      return size;
    }
  }

  public T[] Read()
  {
    using(var accessor = mmf.CreateViewAccessor()){
      int size = accessor.ReadInt32(0); // サイズの読み込み
      var data = new T[size];
      accessor.ReadArray<T>(sizeof(int), data, 0, data.Length);
      return data;
    }
  }
  
  public void Write(int[] data)
  {
    using(var accessor = mmf.CreateViewAccessor()){
      accessor.Write(0, data.Length);
      accessor.WriteArray<int>(sizeof(int), data, 0, data.Length);
    }
  }
  
  public void Close()
  {
    mmf.Dispose();
    mmf = null;
  }

}


public class Pipe {
  protected internal Task task = null;
  CancellationTokenSource tokenSource;

  string path;

  public Pipe() { }

  public void Run(string path, Action<string> Dispatcher, Action<string> Dispatcher2) {
    tokenSource = new CancellationTokenSource();
    var token = tokenSource.Token;

    this.path = path;
    task = Task.Run(() => {
      try{
        while (true) {
          using var stream = new NamedPipeServerStream(path);
          // await stream.WaitForConnectionAsync();
          stream.WaitForConnection();
          using var reader = new StreamReader(stream);
          // var src = await reader.ReadToEndAsync();
          var src = reader.ReadToEnd();
          if (token.IsCancellationRequested) {
            // Thread.Sleep(2000);
            return;
          }
          Dispatcher(src.Trim());
        }
      } catch(Exception e) {
        Dispatcher2(e.ToString());
      }
    }, tokenSource.Token).ContinueWith(t => {
      tokenSource.Dispose( );
      tokenSource = null;
      path = null;
      if(t.IsCanceled) { }
      Dispatcher2("task end");
    });
  }

  public async Task Cancel() {
    tokenSource.Cancel();
    using (var stream = new NamedPipeClientStream(path)) {
      await stream.ConnectAsync(100);
    }
    try {
      await task;
    } catch (TaskCanceledException) {

    }
  }

}