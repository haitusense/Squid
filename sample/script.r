library(jsonlite)
library(ggplot2)
library(svglite)
library(readr)

rutil::ex_hello_world()

dst <- list(id = "id9_out", value = "form R")
json <- toJSON(dst)
json
rutil::win_named_pipe("NamedPipe", json)

data <- ggplot(iris, aes(x = Sepal.Length, y = Sepal.Width)) +
geom_point()
ggsave("temp.svg", plot = data)
svg_string <- read_file("temp.svg")
dst <- list(id = "plot_svg", value = svg_string)
rutil::win_named_pipe("NamedPipe", toJSON(dst))

file.remove("temp.svg")
Sys.sleep(3)
# readline()やsystem("pause")は機能しない
# && pause も使えない
