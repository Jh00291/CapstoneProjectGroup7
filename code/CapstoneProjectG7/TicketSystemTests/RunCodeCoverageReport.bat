@echo off
cd /d "C:\Users\jh00291\Documents\GitHub\CapstoneProjectGroup7\code\CapstoneProjectG7\TicketSystemTests"
echo Running unit tests...
dotnet test --collect:"XPlat Code Coverage"

echo Generating code coverage report...
for /d %%i in (TestResults\*) do set "latestDir=%%i"
reportgenerator -reports:"%latestDir%\coverage.cobertura.xml" -targetdir:"coverageresults" -reporttypes:Html

echo Opening coverage report...
start coverageresults\index.html

echo Done!
pause
