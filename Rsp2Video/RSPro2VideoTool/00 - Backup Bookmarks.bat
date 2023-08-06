@echo off
rem Copy the .FmBok file to .FmBok.bak
for %%f in (*.FmBok) do (
  copy /y "%%~nf.FmBok" "%%~nf.FmBok.bak"
)

rem Copy the .bok file to .bok.bak
for %%f in (*.bok) do (
  copy /y "%%~nf.bok" "%%~nf.bok.bak"
)

echo.
echo Bookmarks backed up.
echo.
pause