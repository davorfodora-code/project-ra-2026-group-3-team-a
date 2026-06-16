@echo off
chcp 65001 >nul
echo ============================================
echo   Trenutni sadrzaj podesavanja aplikacije
echo ============================================
echo.

set "SETTINGS=%APPDATA%\CommunityHub\settings.json"
echo Putanja: %SETTINGS%
echo.

if exist "%SETTINGS%" (
    type "%SETTINGS%"
) else (
    echo [INFO] Fajl jos ne postoji.
    echo        HasSeenWelcome = false  -^>  banner ce se prikazati.
)

echo.
echo.
pause
