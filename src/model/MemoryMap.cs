using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

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


  // public static U Serializer<U>(IList<string> args){
  //   return Parser.Default.ParseArguments<T>(args).WithParsed<T>(op => {
  //     if(op.json is not null){
  //       var buf = JsonSerializer.Deserialize<string>($""" "{op.json}" """);
  //       var dst = JsonSerializer.Deserialize<T>(buf);
  //       var properties = typeof(T).GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
  //       foreach (var prop in properties)
  //       {
  //         var value = prop.GetValue(dst, null);
  //         if (value == null) prop.SetValue(dst, prop.GetValue(op, null), null);
  //       }
  //       return dst;
  //     }else{
  //       return op;
  //     }
  //   });
  // }
