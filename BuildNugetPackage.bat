@REM This script must be executed inside a Developer Command Prompt for Visual Studio

@pushd %~dp0

@echo.
@echo ===== Building CompositionDeviceHelper.dll (x86) =====
@echo.
@msbuild CompositionDeviceHelper\CompositionDeviceHelper.vcxproj /p:Configuration="Release" /p:Platform="x86"

@echo.
@echo ===== Building CompositionDeviceHelper.dll (x64) =====
@echo.
@msbuild CompositionDeviceHelper\CompositionDeviceHelper.vcxproj /p:Configuration="Release" /p:Platform="x64"

@echo.
@echo ===== Building CompositionDeviceHelper.dll (ARM) =====
@echo.
@msbuild CompositionDeviceHelper\CompositionDeviceHelper.vcxproj /p:Configuration="Release" /p:Platform="ARM"

@echo.
@echo ===== Building CompositionProToolkit.dll (x86) =====
@echo.
@msbuild CompositionProToolkit\CompositionProToolkit.csproj /p:Configuration="Release" /p:Platform="x86"

@echo.
@echo ===== Building CompositionProToolkit.dll (x64) =====
@echo.
@msbuild CompositionProToolkit\CompositionProToolkit.csproj /p:Configuration="Release" /p:Platform="x64"

@echo.
@echo ===== Building CompositionProToolkit.dll (ARM) =====
@echo.
@msbuild CompositionProToolkit\CompositionProToolkit.csproj /p:Configuration="Release" /p:Platform="ARM"

@echo.
@echo ===== Building CompositionProToolkit.dll (AnyCPU) =====
@echo.

@REM Create a copy of the x86 CompositionProToolkit.dll and remove the 32-bit flag to make it AnyCPU
@if not exist "CompositionProToolkit\bin\AnyCPU\Release\NUL" mkdir "CompositionProToolkit\bin\AnyCPU\Release"
@copy /Y "CompositionProToolkit\bin\x86\Release\CompositionProToolkit.dll" "CompositionProToolkit\bin\AnyCPU\Release"
@corflags.exe /32BITREQ- "CompositionProToolkit\bin\AnyCPU\Release\CompositionProToolkit.dll"


@echo.
@echo ===== Building CompositionProToolkit.Controls.dll (x86) =====
@echo.
@msbuild CompositionProToolkit.Controls\CompositionProToolkit.Controls.csproj /p:Configuration="Release" /p:Platform="x86"

@echo.
@echo ===== Building CompositionProToolkit.Controls.dll (x64) =====
@echo.
@msbuild CompositionProToolkit.Controls\CompositionProToolkit.Controls.csproj /p:Configuration="Release" /p:Platform="x64"

@echo.
@echo ===== Building CompositionProToolkit.Controls.dll (ARM) =====
@echo.
@msbuild CompositionProToolkit.Controls\CompositionProToolkit.Controls.csproj /p:Configuration="Release" /p:Platform="ARM"

@echo.
@echo ===== Building CompositionProToolkit.Controls.dll (AnyCPU) =====
@echo.

@REM Create a copy of the x86 CompositionProToolkit.dll and remove the 32-bit flag to make it AnyCPU
@if not exist "CompositionProToolkit.Controls\bin\AnyCPU\Release\NUL" mkdir "CompositionProToolkit.Controls\bin\AnyCPU\Release"
@copy /Y "CompositionProToolkit.Controls\bin\x86\Release\CompositionProToolkit.Controls.dll" "CompositionProToolkit.Controls\bin\AnyCPU\Release"
@corflags.exe /32BITREQ- "CompositionProToolkit.Controls\bin\AnyCPU\Release\CompositionProToolkit.Controls.dll"


@echo.
@echo ===== Creating NuGet package for CompositionProToolkit =====
@echo.
@nuget.exe pack nuget\CompositionProToolkit.nuspec

@echo.
@echo ===== Creating NuGet package for CompositionProToolkit.Controls =====
@echo.
@nuget.exe pack nuget\CompositionProToolkit.Controls.nuspec

@popd