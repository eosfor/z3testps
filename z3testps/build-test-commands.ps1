# cd "C:\Users\Andrey_Vernigora\source\repos\z3testps\z3testps\bin\Debug\net6.0"

## buikd
cd (join-path $env:HOMEDRIVE $env:HOMEPATH "source\repos\z3testps\z3testps")
dotnet build && dotnet publish --self-contained --use-current-runtime -c Debug -o c:\temp\z3testps\


## test
cd "c:\temp\z3testps\"
$pid | clip

Import-Module .\z3testps.dll -Verbose
$sourceVMs = Import-Csv .\vmdata.csv
$sourceVMs  | % { $_.cpu = [int]$_.cpu; $_.ram = [int]$_.ram; $_.datadisk = [int]$_.datadisk; }

$targetSizes = import-csv ".\vmCostACUData.csv"


Start-Z3ModelCalculation -SourceVM $sourceVMs -TargetVM $targetSizes