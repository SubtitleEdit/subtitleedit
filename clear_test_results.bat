@echo off
pushd %~p0\src\TestResults
for /d %%g in ("*") do (
    echo removing: %%g
    rd /q /s "%%g"
)
ECHO Test results cleared!
popd
exit /b