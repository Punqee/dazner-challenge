# Builds DAZNer for Windows in Release configuration and copies shared resources.
# Usage: .\windows\build.ps1          builds in Release configuration
#        .\windows\build.ps1 --open   also launches the application

param(
    [switch]$Open,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ExtraArgs
)

$ErrorActionPreference = "Stop"

$openApp = $Open.IsPresent -or ($ExtraArgs -contains "--open") -or ($ExtraArgs -contains "-open")

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$sharedScript = Join-Path $repoRoot "shared\fill-fullscreen.js"
$sharedIcon = Join-Path $repoRoot "shared\assets\icon.png"
$appIcon = Join-Path $scriptDir "app.ico"
$slnPath = Join-Path $scriptDir "DAZNer.sln"

if (-not (Test-Path -LiteralPath $sharedScript)) {
    [Console]::Error.WriteLine("Missing ${sharedScript}: fullscreen emulation requires this file.")
    exit 1
}

if (-not (Test-Path -LiteralPath $appIcon)) {
    if (-not (Test-Path -LiteralPath $sharedIcon)) {
        [Console]::Error.WriteLine("Missing ${sharedIcon}: cannot generate application icon.")
        exit 1
    }

    Add-Type -AssemblyName System.Drawing
    $src = [System.Drawing.Image]::FromFile($sharedIcon)
    $sizes = @(16, 24, 32, 48, 64, 128, 256)
    $pngBlobs = New-Object System.Collections.Generic.List[byte[]]

    foreach ($sz in $sizes) {
        $bmp = New-Object System.Drawing.Bitmap($sz, $sz, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.Clear([System.Drawing.Color]::Transparent)

        $r = [single]($sz * 0.2237)
        $d = [single]($r * 2.0)
        $path = New-Object System.Drawing.Drawing2D.GraphicsPath
        $path.AddArc(0, 0, $d, $d, 180, 90)
        $path.AddArc($sz - $d, 0, $d, $d, 270, 90)
        $path.AddArc($sz - $d, $sz - $d, $d, $d, 0, 90)
        $path.AddArc(0, $sz - $d, $d, $d, 90, 90)
        $path.CloseFigure()

        $g.SetClip($path)
        $g.DrawImage($src, 0, 0, $sz, $sz)
        $g.Dispose()
        $path.Dispose()

        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngBlobs.Add($ms.ToArray())
        $ms.Dispose()
        $bmp.Dispose()
    }

    $src.Dispose()

    $outStream = [System.IO.File]::Create($appIcon)
    $bw = New-Object System.IO.BinaryWriter($outStream)
    $bw.Write([uint16]0)
    $bw.Write([uint16]1)
    $bw.Write([uint16]$sizes.Count)

    $offset = 6 + (16 * $sizes.Count)
    for ($i = 0; $i -lt $sizes.Count; $i++) {
        $sz = $sizes[$i]
        $blob = $pngBlobs[$i]
        $dimByte = if ($sz -ge 256) { [byte]0 } else { [byte]$sz }
        $bw.Write($dimByte)
        $bw.Write($dimByte)
        $bw.Write([byte]0)
        $bw.Write([byte]0)
        $bw.Write([uint16]1)
        $bw.Write([uint16]32)
        $bw.Write([uint32]$blob.Length)
        $bw.Write([uint32]$offset)
        $offset += $blob.Length
    }

    foreach ($blob in $pngBlobs) {
        $bw.Write($blob)
    }

    $bw.Flush()
    $bw.Dispose()
    $outStream.Dispose()
}

& dotnet build $slnPath -c Release
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$outDir = Join-Path $scriptDir "bin\Release\net10.0-windows"
$exePath = Join-Path $outDir "DAZNer.exe"
$destScript = Join-Path $outDir "fill-fullscreen.js"

if (-not (Test-Path -LiteralPath $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}
Copy-Item -LiteralPath $sharedScript -Destination $destScript -Force

if (-not (Test-Path -LiteralPath $exePath)) {
    [Console]::Error.WriteLine("Build finished, but executable was not found at $exePath")
    exit 1
}

Write-Output "Ready: $exePath"

if ($openApp) {
    Start-Process -FilePath $exePath
}
