@echo off
chcp 65001 >nul
echo ============================================
echo   Reset uvodnog (welcome) banera
echo ============================================
echo.

set "SETTINGS=%APPDATA%\CommunityHub\settings.json"

if exist "%SETTINGS%" (
    del "%SETTINGS%"
    echo [OK] Obrisano: %SETTINGS%
    echo      Banner ce se ponovo prikazati pri sledecem pokretanju.
) else (
    echo [INFO] Fajl ne postoji - banner ce se ionako prikazati.
)

echo.
echo Sada pokreni aplikaciju.
echo.
pause
