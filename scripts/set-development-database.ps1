param(
    [Parameter(Mandatory = $true)]
    [string]$ConnectionString
)

$value = $ConnectionString.Trim()
if ($value.Length -ge 2 -and
    (($value[0] -eq '"' -and $value[-1] -eq '"') -or
     ($value[0] -eq "'" -and $value[-1] -eq "'"))) {
    $value = $value.Substring(1, $value.Length - 2).Trim()
}

$project = Join-Path $PSScriptRoot '..\src\TechNotes.API\TechNotes.API.csproj'
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' $value --project $project
