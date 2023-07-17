
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

var hoge = new MemoryMap<int>("mmf", Enumerable.Range(0, 10).Select(x => x *2).ToArray<int>());

Console.WriteLine(hoge.Length());

var foo = hoge.Read();
Console.WriteLine(foo[0]);
Console.WriteLine(foo[1]);
Console.WriteLine(foo[2]);
Console.WriteLine(foo[3]);

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
  
  
  public void Clear()
  {
    mmf.Dispose();
    mmf = null;
  }

}
