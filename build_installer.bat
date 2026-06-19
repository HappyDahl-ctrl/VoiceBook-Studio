@echo off
setlocal

set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
set PROJECT=VoiceBookStudio.csproj
set PUBLISH_DIR=publish
set ISS=VoiceBookStudio.iss

echo.
echo ============================================================
echo  VoiceBook Studio — Build Installer
echo ============================================================
echo.

:: ── Step 1: dotnet publish ───────────────────────────────────
echo [1/2] Publishing .NET application...
echo.

dotnet publish "%PROJECT%" ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  --output "%PUBLISH_DIR%" ^
  -p:PublishReadyToRun=true

if errorlevel 1 (
    echo.
    echo ERROR: dotnet publish failed. Check the output above.
    pause
    exit /b 1
)

echo.
echo Publish succeeded.

:: ── Step 2: Copy Data folder (prompts) ──────────────────────
echo.
echo Ensuring Data\PromptLibrary is in publish output...
if not exist "%PUBLISH_DIR%\Data\PromptLibrary" (
    mkdir "%PUBLISH_DIR%\Data\PromptLibrary"
)
copy /Y "Data\PromptLibrary\prompts.json" "%PUBLISH_DIR%\Data\PromptLibrary\prompts.json" >nul
echo Done.

:: ── Step 3: Inno Setup ──────────────────────────────────────
echo.
echo [2/2] Building installer with Inno Setup...
echo.

if not exist %ISCC% (
    echo ERROR: Inno Setup not found at %ISCC%
    echo Install Inno Setup 6 from https://jrsoftware.org/isdl.php
    pause
    exit /b 1
)

%ISCC% "%ISS%"

if errorlevel 1 (
    echo.
    echo ERROR: Inno Setup build failed.
    pause
    exit /b 1
)

echo.
echo ============================================================
echo  SUCCESS: Installer built in the Installer\ folder.
echo ============================================================
echo.

:: Open the Installer folder in Explorer
explorer Installer

endlocal
