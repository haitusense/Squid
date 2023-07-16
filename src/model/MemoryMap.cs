using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace Squid;

public class MemoryMap {
  MemoryMappedFile mmf;

  public void Set(string key, int width, int height, int[] data)
  {
    mmf = MemoryMappedFile.CreateNew(key, (width*height+1) * sizeof(int));
    using(var accessor = mmf.CreateViewAccessor()){
      accessor.Write(0, data.Length); // サイズの書き込み
      int offset = 0;
      foreach(int i in data){
        accessor.Write((++offset)*sizeof(int), i);
        // accessor.WriteArray<int>(sizeof(int), new int[]{ (int)i }, offset, 1);
      }
    }
  }

  public int[] Read()
  {
    using(var accessor = mmf.CreateViewAccessor()){
      int size = accessor.ReadInt32(0); // サイズの読み込み
      var data = new int[size];
      accessor.ReadArray<int>(sizeof(int), data, 0, data.Length);
      return data;
    }
  }

  public void Write(int[] data)
  {
    using(var accessor = mmf.CreateViewAccessor()){
      accessor.Write(0, data.Length); // サイズの書き込み
      accessor.WriteArray<int>(sizeof(int), data, 0, data.Length);
    }
  }

  public void Clear()
  {
    mmf.Dispose();
    mmf = null;
  }

}


public class MemoryMap2<T> where T : struct {
  private string mapname = "shared_memory";
  public T[] data;

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

  public void Read(){
    using(var mmf = MemoryMappedFile.OpenExisting(mapname)){
      using(var accessor = mmf.CreateViewAccessor()){
        int size = accessor.ReadInt32(0);
        data = new T[size];
        accessor.ReadArray<T>(sizeof(int), data, 0, data.Length);
        // int offset = 0;
        // foreach(int i in data){
        //   accessor.Write((++offset)*sizeof(T), (int)100);
        // }
      }
    }
  }

  public void Write(){
    using(var mmf = MemoryMappedFile.OpenExisting(mapname)){
      using(var accessor = mmf.CreateViewAccessor()){
        accessor.Write(0, data.Length); // サイズの書き込み
        accessor.WriteArray<T>(sizeof(int), data, 0, data.Length);
      }
    }
  }

}
