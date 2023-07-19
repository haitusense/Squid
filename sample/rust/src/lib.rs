use std::io::Write;
use named_pipe::PipeClient;
use std::os::windows::ffi::OsStrExt;
use winapi;

fn pipe(path:&str, message:&str) -> anyhow::Result<()> {

  // let pipe_path = r"\\.\pipe\NamedPipe";
  let path = format!(r##"\\.\pipe\{path}"##);
  let mut pipe = PipeClient::connect(path)?;

  // let message_bytes = br##"{ "id" : "id", "value" : "from rust" }"##;
  let message_bytes = message.as_bytes();

  pipe.write_all(message_bytes)?;
  
  Ok(())
}

fn mmf(path:&str) -> anyhow::Result<()> {
  let wide_name: Vec<u16> = std::ffi::OsStr::new(path)
  .encode_wide()
  .chain(std::iter::once(0))
  .collect();
  let handle = unsafe { kernel32::OpenFileMappingW(
    winapi::um::winnt::SECTION_MAP_WRITE,
    winapi::shared::minwindef::FALSE,
    wide_name.as_ptr()
  ) };
  if handle.is_null() { anyhow::bail!("mmf err"); }

  let view = unsafe { kernel32::MapViewOfFile(
    handle,
    winapi::um::winnt::SECTION_MAP_WRITE,
    0,
    0,
    0,
  ) };

  if view.is_null() {
    unsafe { kernel32::CloseHandle(handle) };
    anyhow::bail!("mmf err");
  }
  let memory_slice = unsafe {
    std::slice::from_raw_parts_mut(view as *mut i32, 640 * 480 + 1)
  };
  let a = memory_slice[0];
  let b = memory_slice[1];
  let c = memory_slice[2];
  println!("{a} {b} {c}");
  
  unsafe {
    kernel32::UnmapViewOfFile(view);
    kernel32::CloseHandle(handle) 
  };
  Ok(())
}

#[cfg(test)]
mod tests {
  use crate::*;
  
  #[test]
  fn it_works() {
    let value = "from rust5";
    let message = format!(r##"{{ "id" : "id7_out", "value" : "{value}" }}"##);
    pipe("NamedPipe", message.as_str()).unwrap();
  }

  #[test]
  fn it_works_mmf() {
    mmf("mmf").unwrap();

  }

}