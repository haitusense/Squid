library(jsonlite)
library(ggplot2)
library(gridSVG)
library(XML)

args <- jsonlite::fromJSON(commandArgs()[6])
args

dst <- list(id = args$id, value = "form R")
rsquid::namedPipe("NamedPipe", toJSON(dst))

pdf(NULL) # Rplots.pdf の作成を抑制
p <- ggplot(iris, aes(x = Sepal.Length, y = Sepal.Width)) + geom_point()
p
svg <- grid.export(NULL, addClasses = TRUE, prefix = "gridsvg")$svg
svg <- XML::removeAttributes(svg, "width")
svg <- XML::addAttributes(svg, "height" = "100%")
svg <- XML::removeChildren(svg, "metadata")

dst <- list(id = "plot_svg", value = saveXML(svg))
rsquid::namedPipe("NamedPipe", toJSON(dst))

Sys.sleep(3)
# readline()やsystem("pause")は機能しない
# && pause も使えない
