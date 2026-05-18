param(
    [string]$ProjectDir = ".\HotelGuestEditor"
)

$ErrorActionPreference = "Stop"

$form2Path = Join-Path $ProjectDir "Form2.cs"
$resultPath = Join-Path $ProjectDir "IdentityGuestSearchResult.cs"
$form1Path = Join-Path $ProjectDir "Form1.cs"

if (!(Test-Path $form2Path)) { throw "Form2.cs not found at $form2Path" }
if (!(Test-Path $resultPath)) { throw "IdentityGuestSearchResult.cs not found at $resultPath" }
if (!(Test-Path $form1Path)) { throw "Form1.cs not found at $form1Path" }

$form2 = Get-Content $form2Path -Raw -Encoding UTF8
$result = Get-Content $resultPath -Raw -Encoding UTF8
$form1 = Get-Content $form1Path -Raw -Encoding UTF8

$oldDobBlock = @'
    CASE 
        WHEN G.GSTBTH <> 0 
            THEN CONVERT(DATETIME, CONVERT(CHAR(8), G.GSTBTH))
        ELSE NULL
    END AS [Date Of Birth],
'@

$newDobBlock = @'
    CASE 
        WHEN G.GSTBTH <> 0 
            THEN CONVERT(DATE, CONVERT(CHAR(8), G.GSTBTH), 112)
        ELSE NULL
    END AS [Date Of Birth],
'@

if ($form2.Contains($oldDobBlock)) {
    $form2 = $form2.Replace($oldDobBlock, $newDobBlock)
}
elseif ($form2 -notmatch "\[Date Of Birth\]") {
    $insertAfter = "COALESCE(A.GSTCOD, 0) AS [Guest Code],"
    $form2 = $form2.Replace($insertAfter, $insertAfter + "`r`n" + $newDobBlock)
}

if ($form2 -notmatch "PMS\.GSPERTBL G") {
    $joinAnchor = "LEFT JOIN PMS.GSPASTBL P"
    $gJoin = @'

LEFT JOIN PMS.GSPERTBL G
    ON G.GSTCOD = A.GSTCOD
   AND G.MODCOD = A.MODCOD

'@
    $form2 = $form2.Replace($joinAnchor, $gJoin + $joinAnchor)
}

if ($form2 -notmatch "\[Guest Email\]") {
    $form2 = $form2 -replace "COALESCE\(I\.FUTU04, ''\) AS \[Work Permit\],", "COALESCE(I.FUTU04, '') AS [Work Permit],`r`n    COALESCE(E.GSTEML, '') AS [Guest Email],"
}

Set-Content $form2Path $form2 -Encoding UTF8

if ($result -notmatch "DateTime\? DateOfBirth") {
    $result = $result -replace "public string Passport \{ get; private set; \} = string\.Empty;", "public string Passport { get; private set; } = string.Empty;`r`n        public DateTime? DateOfBirth { get; private set; }"
}

if ($result -notmatch "DateOfBirth = ReadDate\(row, ""Date Of Birth""\)") {
    $result = $result -replace "Passport = ReadString\(row, ""Passport""\),", "Passport = ReadString(row, ""Passport""),`r`n                DateOfBirth = ReadDate(row, ""Date Of Birth""),"
}

if ($result -notmatch "private static DateTime\? ReadDate") {
    $readDateMethod = @'

        private static DateTime? ReadDate(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return null;

            if (row[columnName] is DateTime date)
                return date;

            if (DateTime.TryParse(row[columnName]?.ToString(), out DateTime parsed))
                return parsed;

            return null;
        }
'@
    $result = $result -replace "\r?\n\s*\}\r?\n\}\r?\n$", $readDateMethod + "`r`n    }`r`n}`r`n"
}

Set-Content $resultPath $result -Encoding UTF8

if ($form1 -match "ApplyIdentitySearchResultToForm\(IdentityGuestSearchResult guest\)" -and
    $form1 -notmatch "guest\.DateOfBirth\.HasValue") {

    $anchor = 'txtEmail.Text = guest.GuestEmail;'
    $dobApply = @'

            if (guest.DateOfBirth.HasValue)
            {
                chkBirthDate.Checked = true;
                dtBirthDate.Value = guest.DateOfBirth.Value;
            }
            else
            {
                chkBirthDate.Checked = false;
            }
'@

    if ($form1.Contains($anchor)) {
        $form1 = $form1.Replace($anchor, $anchor + $dobApply)
    }
}

Set-Content $form1Path $form1 -Encoding UTF8

Write-Host "Form2 Date Of Birth support was applied successfully."
