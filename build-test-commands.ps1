## run dotnet build in the project folder
cd (join-path $env:HOMEDRIVE $env:HOMEPATH "source\repos\z3testps\z3testps")
dotnet build && dotnet publish --self-contained --use-current-runtime -c Debug -o c:\temp\z3testps\

## test
cd "c:\temp\z3testps\"
Import-Module .\z3testps.dll -Verbose
$pid | clip


$sourceVMs = Import-Csv .\vmdata.csv
$sourceVMs  | % { $_.cpu = [int]$_.cpu; $_.ram = [int]$_.ram; $_.datadisk = [int]$_.datadisk; }

$targetSizes = import-csv ".\vmCostACUData.csv"


## run with only one source VM
$m = Start-Z3ModelCalculation -SourceVM $sourceVMs[0] -TargetVM $targetSizes
$m.Consts | % {$_.Tostring()}

## run with all source VMs
$m = Start-Z3ModelCalculation -SourceVM $sourceVMs -TargetVM $targetSizes