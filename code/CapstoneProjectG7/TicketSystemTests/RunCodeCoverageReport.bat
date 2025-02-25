@echo off
cd /d "C:\Users\jh00291\Documents\GitHub\CapstoneProjectGroup7\code\CapstoneProjectG7\TicketSystemTests"
echo Running unit tests...
dotnet test --collect:"XPlat Code Coverage"

:: Wait for test execution to finish
timeout /t 5 /nobreak >nul

:: Find the newest test result directory
for /f "delims=" %%i in ('dir /b /ad /o-d "TestResults" 2^>nul') do set "latestDir=TestResults\%%i" & goto :found
echo No test results found!
pause
exit /b 1
:found

echo Generating code coverage report...
reportgenerator -reports:"%latestDir%\coverage.cobertura.xml" -targetdir:"coverageresults" -reporttypes:Html

:: Ensure the report exists before opening
if exist "coverageresults\index.html" (
    echo Opening coverage report...
    start "" "coverageresults\index.html"
) else (
    echo Coverage report generation failed!
)

echo Done!
pause
