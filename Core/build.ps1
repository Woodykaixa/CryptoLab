function BuildDll {
    param (
        [string]$SourceMainFile,
        [array]$SourceOtherFiles,
        [string]$ObjectFile
    )
    $SourceFiles = $SourceMainFile, $SourceOtherFiles;
    $Compiler = "clang";
    $StandardFlag = "-std=c11";
    if ($SourceMainFile.Contains(".cpp")) {
        $Compiler = "clang++";
        $StandardFlag = "-std=c++17";
    }
    $DoBuild = $false;
    try {
        $o = Get-ItemProperty -Path $ObjectFile -ErrorAction SilentlyContinue;
        
        foreach ($source in $SourceFiles) {
            $s = Get-ItemProperty -Path $source -ErrorAction SilentlyContinue;
            if ($s.LastWriteTime -gt $o.LastWriteTime) {
                $DoBuild = $true;
                break;
            }
        }
    }
    catch {
    }
    if ($DoBuild) {
        $BuildCommand = "{0} .\{1} -o {2} {3} -shared" -f $Compiler, $SourceMainFile , $ObjectFile, $StandardFlag;
        Invoke-Expression $BuildCommand;
        Write-Output ("Updated {0}" -f $ObjectFile);
        Copy-Item $ObjectFile ../
    }
    else {
        Write-Output ("{0} is up to date." -f $ObjectFile);
    }
 
}

Set-Location .\Caesar
BuildDll "caesar.c" "caesar.h" "Core.Caesar.dll"

Set-Location ..\DES
BuildDll "des.cpp" "des.h" "Core.DES.dll"

Set-Location ..\Monoalphabetic
BuildDll "substitution.cpp" "substitution.h" "Core.Monoalphabetic.dll"

Set-Location ..\RSA
BuildDll "rsa.cpp" "rsa.h", "RSAPrimes.h" "Core.RSA.dll"

Set-Location ..\