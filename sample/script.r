library(jsonlite)

rutil::ex_hello_world()

dst <- list(id = "id9_out", value = "form R")
json <- toJSON(dst)
json
rutil::win_named_pipe("NamedPipe", json)

Sys.sleep(3) 
# readline()やsystem("pause")は機能しない
# && pause も使えない
