library(jsonlite)
library(ggplot2)
library(gridSVG)
library(XML)

dst <- list(id = "id9_out", value = "form R")
rsquid::named_pipe("NamedPipe", toJSON(dst))

p <- ggplot(iris, aes(x = Sepal.Length, y = Sepal.Width)) + geom_point()
p
svg <- saveXML(grid.export(NULL, addClasses = TRUE, prefix = "ggridsvg")$svg)
dst <- list(id = "plot_svg", value = svg)
rsquid::named_pipe("NamedPipe", toJSON(dst))

Sys.sleep(3)
# readline()やsystem("pause")は機能しない
# && pause も使えない
