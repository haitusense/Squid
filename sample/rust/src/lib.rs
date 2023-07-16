use std::io::Write;
use named_pipe::PipeClient;

fn pipe(path:&str, message:&str) -> anyhow::Result<()> {

  // let pipe_path = r"\\.\pipe\NamedPipe";
  let path = format!(r##"\\.\pipe\{path}"##);
  let mut pipe = PipeClient::connect(path)?;

  // let message_bytes = br##"{ "id" : "id", "value" : "from rust" }"##;
  let message_bytes = message.as_bytes();

  pipe.write_all(message_bytes)?;
  
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

}