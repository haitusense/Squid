library(ggplot2)
library(gridSVG)
library(XML)
library(dplyr)

pdf(NULL)
args <- jsonlite::fromJSON(commandArgs()[6])
args

Sys.sleep(5)

width <- 700
height <- 700
src <- rsquid::readMemoryMappedFile("mmf")
x <- rep(1:width, length.out = width * height)
y <- rep(1:height, each = width)
df <- data.frame(data = src, x = x, y = y)

if (args$flag == "histogram") {
  g <- ggplot(df, aes(x = data)) + geom_histogram()
} else if (args$flag == "line") {
  plot_data <- filter(df, 30 <= y & y <= 31)
  g <- ggplot(plot_data, aes(x, data)) +
  scale_y_continuous(limits = c(200, 255)) +
  scale_x_continuous(limits = c(0, 700)) +
  geom_point() +
  theme_minimal()
}
g
svg <- grid.export(NULL, addClasses = TRUE, prefix = "gridsvg")$svg
svg <- XML::removeAttributes(svg, "width")
svg <- XML::addAttributes(svg, "height" = "100%")
svg <- XML::removeChildren(svg, "metadata")

dst <- list(id = "plot", value = saveXML(svg, indent = FALSE))
rsquid::namedPipe("NamedPipe", jsonlite::toJSON(dst))

Sys.sleep(5)