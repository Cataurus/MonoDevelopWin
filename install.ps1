# aktuelles verzeichnis
$dir = (Get-location).ToString()

$monoBasePath = ${env:ProgramFiles(x86)} + '\Mono\';
$monBinPath = $monoBasePath + 'bin';

$gtksharpPath = ${env:ProgramFiles(x86)} + '\gtksharp\';

Write-host " "
Write-Host "Lade Gtk-Sharp herunter..."
$targetGtkSharp = $dir + '\gtk-sharp-2.12.45.msi'

(new-object net.webclient).DownloadFile('https://xamarin.azureedge.net/GTKforWindows/Windows/gtk-sharp-2.12.45.msi', $targetGtkSharp)

Write-host " "
Write-Host "Installiere Gtk-Sharp ..."
cmd /c start /wait msiexec /i $targetGtkSharp /quiet /qn /norestart

Write-host " "
Write-Host "Lade Mono-gtk herunter..."
$targetMono = $dir + '\mono-6.12.0.107-gtksharp-2.12.45-win32-0.msi'

(new-object net.webclient).DownloadFile('https://download.mono-project.com/archive/6.12.0/windows-installer/mono-6.12.0.107-gtksharp-2.12.45-win32-0.msi', $targetMono)

Write-host " "
Write-Host "Installiere Mono-gtk ..."

cmd /c start /wait msiexec /i "$targetMono" /quiet /qn /norestart

Write-host " "
Write-Host "Lade MonoLib herunter..."

$targetMonoLib = $dir + '\MonoLibraries.msi'
(new-object net.webclient).DownloadFile('https://files.xamarin.com/~jeremie/MonoLibraries.msi', $targetMonoLib)

Write-host " "
Write-Host "Installiere MonoLib ..."

cmd /c start /wait msiexec /i "$targetMonoLib" /quiet /qn /norestart 

Write-host " "
Write-Host "Lade GetText herunter..."
$targetGetText = $dir + '\gettext-0.14.4.exe'

(new-object net.webclient).DownloadFile('https://downloads.sourceforge.net/project/gnuwin32/gettext/0.14.4/gettext-0.14.4.exe?r=https%3A%2F%2Fsourceforge.net%2Fprojects%2Fgnuwin32%2Ffiles%2Fgettext%2F0.14.4%2Fgettext-0.14.4.exe%2Fdownload%3Fuse_mirror%3Ddeac-riga%26download%3D&ts=1608399707', $targetGetText)

Write-host " "
Write-Host "Installiere MonoLib ..."
cmd /c start /wait msiexec /i "$targetGetText" /quiet /qn /norestart 

#Write-host " "
#Write-Host "Lade MonoDoc herunter..."
#$targetMonoDoc = $dir + '\monodoc.dll'

#(new-object net.webclient).DownloadFile('https://github.com/lextm/monodevelop-windows/blob/master/monodoc.dll', $targetMonoDoc)
# & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $targetMonoDoc

Write-host " "

if(Test-Path "$monBinPath") {
    Write-host "Mono installed" -ForegroundColor Green
}

Write-host " "
$monoExe = $monBinPath+"mono.exe"
& 'C:\Program Files (x86)\Mono\bin\mono.exe' --version
Write-host " "

$WindowsSDK = 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe'
Write-host "Suche nach gacutil"
Write-host " "

if (Test-Path -Path "$WindowsSDK")
{
    Write-host "Wurde gefunden..." -ForegroundColor Green
    Write-host ""
    Write-host "Installiere Bibliotheken im GAC"
    Write-host ""
    Write-host "Gtk+ Bibliotheken werden registriert"
    Write-host ""
    $pfad = $gtksharpPath + '2.12\lib\gtk-sharp-2.0\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    Write-host "Mono.Cairo Bibliotheken werden registriert"
    Write-host ""

    $pfad = $gtksharpPath + '2.12\lib\mono.cairo\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    Write-host "Mono.Posix Bibliotheken werden registriert"
    Write-host ""

    $pfad = $gtksharpPath + '2.12\lib\mono.posix\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    #$pfad = $monoBasePath + '\lib\atk\atk-sharp.dll'
    #& 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $pfad

    Write-host "Mono Bibliotheken werden registriert"
    Write-host ""

    $pfad = $monoBasePath + '\lib\cairo\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    $pfad = $monoBasePath + '\lib\gdk\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    $pfad = $monoBasePath + '\lib\glade\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    $pfad = $monoBasePath + '\lib\glib\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    $pfad = $monoBasePath + '\lib\gtk\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    $pfad = $monoBasePath + '\lib\gtk-dotnet\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    } 

    $pfad = $monoBasePath + '\lib\pango\*.dll'
    $list = (get-childitem -Path $pfad -Recurse)
    foreach ($item in $list)
    {
        $name = $item.Name
        write-host "$name"
        & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $item
    }

    write-host "mono.posix"
    $pfad = $monoBasePath + '\lib\mono\4.5\mono.posix.dll'
    & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $pfad 

    write-host "mono.cairo"
    $pfad = $monoBasePath + '\lib\mono\4.5\mono.cairo.dll'
    & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $pfad
    
    write-host "monodoc"
    $pfad = $monoBasePath + '\lib\mono\monodoc\monodoc.dll'
    & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\gacutil.exe' /nologo /i $pfad 
}
else
{
    Write-host "Konnte nicht gefunden werden, installieren Sie das Windows 10 SDK!" -ForegroundColor Red

    throw "No Windows 10 SDK"
}