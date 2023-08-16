
extern {
  fn date_now() -> f64;
  fn console_log(ptr: *const u16, len: usize);
  fn test() -> *const u8;
}

#[no_mangle]
pub fn get_timestamp() -> f64 {
  unsafe {
    date_now()
  }
}

#[no_mangle]
pub fn add(a: i32, b: i32) -> i32 {
  a + b
}

#[no_mangle]
pub fn console_write() {
  let utf16: Vec<u16> = String::from("こんにちわ").encode_utf16().collect();
  unsafe {
    console_log(utf16.as_ptr(), utf16.len());
  }
}

#[no_mangle]
pub fn test() {
  let utf16: Vec<u16> = String::from("こんにちわ").encode_utf16().collect();
  unsafe {
    console_log(utf16.as_ptr(), utf16.len());
  }
}

/*
https://zenn.dev/a24k/articles/20221012-wasmple-simple-console
https://wasm-dev-book.netlify.app/hello-wasm.html#%E6%9A%97%E9%BB%99%E3%81%AE%E5%9E%8B%E5%A4%89%E6%8F%9B
https://rustwasm.github.io/docs/book/what-is-webassembly.html#linear-memory
https://zenn.dev/a24k/articles/20221107-wasmple-passing-buffer
*/