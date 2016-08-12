@REM This script must be executed inside a Developer Command Prompt for Visual Studio
@REM This script must be present in the Solution Folder of the CompositionProToolkit project.

@pushd %~dp0

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
@echo ===== Updating Reference file =====
@copy /Y CompositionProToolkit\bin\x86\Release\CompositionProToolkit.cs CompositionProToolkit.Ref

@echo.
@echo ===== Building CompositionProToolkit.dll (reference) =====
@echo.
@msbuild CompositionProToolkit.Ref\CompositionProToolkit.Ref.csproj /p:Configuration="Release" /p:Platform="AnyCPU"

@copy /Y CompositionProToolkit\bin\x86\Release\*.pri NuGet\ref\uap10.0
@copy /Y CompositionProToolkit\bin\x86\Release\*.xr.xml NuGet\ref\uap10.0\CompositionProToolkit

@copy /Y CompositionProToolkit\Themes\*.xaml NuGet\ref\uap10.0\CompositionProToolkit\Themes

@echo.
@echo ===== Creating NuGet package =====
@echo.
@nuget.exe pack NuGet\CompositionProToolkit.nuspec

@popd

