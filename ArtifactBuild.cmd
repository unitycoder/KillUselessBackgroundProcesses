@echo off
pushd "%~dp0"
powershell Compress-7Zip "KillUselessBackgroundProcesses\nin\Release\*" -ArchiveFileName "KillUselessBackgroundProcessesX64.zip" -Format Zip
:exit
popd
@echo on
