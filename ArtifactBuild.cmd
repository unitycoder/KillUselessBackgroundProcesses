@echo off
pushd "%~dp0"
powershell Compress-7Zip "KillUselessBackgroundProcesses\Bin\x64\Release" -ArchiveFileName "KillUselessBackgroundProcessesX64.zip" -Format Zip
:exit
popd
@echo on
