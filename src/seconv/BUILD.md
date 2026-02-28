# Building and Publishing SeConv

## Development Build

```bash
# Build the project
dotnet build SeConv/SeConv.csproj

# Run during development
dotnet run --project SeConv/SeConv.csproj -- [arguments]
```

## Publish for Distribution

### Windows (Self-Contained)
```bash
dotnet publish SeConv/SeConv.csproj -c Release -r win-x64 --self-contained -o publish/win-x64
```

### Linux (Self-Contained)
```bash
dotnet publish SeConv/SeConv.csproj -c Release -r linux-x64 --self-contained -o publish/linux-x64
```

### macOS (Self-Contained)
```bash
dotnet publish SeConv/SeConv.csproj -c Release -r osx-x64 --self-contained -o publish/osx-x64
```

### Framework-Dependent (Requires .NET 10 Runtime)
```bash
dotnet publish SeConv/SeConv.csproj -c Release -o publish/framework-dependent
```

### Single File Executable
```bash
# Windows
dotnet publish SeConv/SeConv.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/single-file

# Linux
dotnet publish SeConv/SeConv.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish/single-file

# macOS
dotnet publish SeConv/SeConv.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o publish/single-file
```

## After Publishing

The executable will be named:
- Windows: `SeConv.exe`
- Linux/macOS: `SeConv`

You can then run it directly:

```bash
# Windows
SeConv.exe convert *.srt sami

# Linux/macOS
./SeConv convert *.srt sami
```

## Creating an Alias (Optional)

### Windows (PowerShell Profile)
Add to your PowerShell profile:
```powershell
Set-Alias -Name SubtitleEdit -Value "C:\path\to\SeConv.exe"
```

### Linux/macOS (Bash/Zsh)
Add to your `.bashrc` or `.zshrc`:
```bash
alias SubtitleEdit='/path/to/SeConv'
```

Then you can use it as shown in the documentation:
```bash
SubtitleEdit convert *.srt sami
SubtitleEdit formats
SubtitleEdit --help
```

## Distribution

Consider distributing via:
1. **GitHub Releases** - Attach binaries to releases
2. **Chocolatey** (Windows) - Create a package
3. **Homebrew** (macOS) - Create a formula
4. **apt/yum** (Linux) - Create distribution packages
5. **.NET Tool** - Publish as a global tool:
   ```bash
   dotnet pack SeConv/SeConv.csproj
   dotnet tool install --global --add-source ./nupkg SeConv
   ```
