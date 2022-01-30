## git clone somewhere

## run dotnet build in the project folder
## it will put everything into c:\temp\z3testps\
cd (join-path $env:HOMEDRIVE $env:HOMEPATH "source\repos\z3testps\z3testps")
dotnet build && dotnet publish --self-contained --use-current-runtime -c Debug -o c:\temp\z3testps\

## then simply open powershell
## copy data files - vmdata.csv and vmCostACUData.csv to the output folder c:\temp\z3testps\
## cd into it and you can run it

cd "c:\temp\z3testps\"
Import-Module .\z3testps.dll -Verbose
$pid | clip ## this is to put the pwsh process id into the clipboard, so you can then pasete it to the VS to be able to attach to the process


$sourceVMs = Import-Csv .\vmdata.csv
$sourceVMs  | % { $_.cpu = [int]$_.cpu; $_.ram = [int]$_.ram; $_.datadisk = [int]$_.datadisk; }

$targetSizes = import-csv ".\vmCostACUData.csv"

## Z3 RELATED SECTION
## run with only one source VM
$m = Start-Z3ModelCalculation -SourceVM $sourceVMs[0] -TargetVM $targetSizes
$m.Consts | % {$_.Tostring()}

## run with all source VMs - this run forever 
$m = Start-Z3ModelCalculation -SourceVM $sourceVMs -TargetVM $targetSizes

## see results
$m.Decls | ? { $m.ConstInterp($_).ToString() -ne "0" } | % { "$($_.Name) -> $($m.ConstInterp($_))" }

## OR-TOOLS RELATED SECTION
$x = Start-OrToolsModelCalculation -SourceVM $sourceVMs -TargetVM $targetSizes

## see results
$x[0][1].VmMappingResult | ft -AutoSize