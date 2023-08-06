del palette.png
ffmpeg -i animation%%02d.png -vf palettegen palette.png
ffmpeg -y -framerate 30 -i animation%%02d.png -i palette.png -lavfi paletteuse -sws_dither none animation.gif
del *.png
