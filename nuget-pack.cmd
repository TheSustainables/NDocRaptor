@echo off
del *.nupkg

nuget pack -Prop Configuration=Release -Build

pause