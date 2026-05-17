param(
    [string]$ProjectDir = ".\HotelGuestEditor"
)

$ErrorActionPreference = "Stop"

$formPath = Join-Path $ProjectDir "Form1.cs"
$designerPath = Join-Path $ProjectDir "Form1.Designer.cs"
$csprojPath = Join-Path $ProjectDir "HotelGuestEditor.csproj"

if (!(Test-Path $formPath)) { throw "Form1.cs not found at $formPath" }
if (!(Test-Path $designerPath)) { throw "Form1.Designer.cs not found at $designerPath" }

$form = Get-Content $formPath -Raw -Encoding UTF8
$designer = Get-Content $designerPath -Raw -Encoding UTF8

# --------------------------------------------------------------------
# Form1.Designer.cs: add Search ID button as a real designer control
# --------------------------------------------------------------------
if ($designer -notmatch "btnIdentitySearch = new Button\(\);") {
    $designer = $designer -replace "btnLoadSelectedGuest = new Button\(\);", "btnLoadSelectedGuest = new Button();`r`n            btnIdentitySearch = new Button();"
}

if ($designer -notmatch "// btnIdentitySearch") {
    $identityButtonBlock = @'
            // 
            // btnIdentitySearch
            // 
            btnIdentitySearch.Location = new Point(822, 350);
            btnIdentitySearch.Name = "btnIdentitySearch";
            btnIdentitySearch.Size = new Size(95, 28);
            btnIdentitySearch.TabIndex = 126;
            btnIdentitySearch.Text = "Search ID";
            btnIdentitySearch.UseVisualStyleBackColor = true;
            btnIdentitySearch.Visible = false;
            btnIdentitySearch.Click += btnIdentitySearch_Click;
            // 

'@

    $replacement = $identityButtonBlock + "            // `r`n            // lblGstCod"
    $designer = $designer -replace "            // \r?\n            // lblGstCod", $replacement
}

if ($designer -notmatch "Controls\.Add\(btnIdentitySearch\);") {
    $designer = $designer -replace "Controls\.Add\(btnLoadSelectedGuest\);", "Controls.Add(btnIdentitySearch);`r`n            Controls.Add(btnLoadSelectedGuest);"
}

if ($designer -notmatch "private .*Button btnIdentitySearch;") {
    $designer = $designer -replace "private System\.Windows\.Forms\.Button btnLoadSelectedGuest;", "private System.Windows.Forms.Button btnLoadSelectedGuest;`r`n        private System.Windows.Forms.Button btnIdentitySearch;"
}

Set-Content $designerPath $designer -Encoding UTF8

# --------------------------------------------------------------------
# Form1.cs: include designer button in visibility group, add handlers
# --------------------------------------------------------------------
if ($form -match "InitializeIdentitySearchButton\(\);") {
    $form = $form -replace "\s*InitializeIdentitySearchButton\(\);\r?\n", "`r`n"
}

$form = $form -replace "\s*private Button _btnIdentitySearch;\r?\n", "`r`n"

if ($form -notmatch "btnLoadSelectedGuest, btnIdentitySearch") {
    $form = $form -replace "lblGuests, cmbGuests, btnLoadSelectedGuest", "lblGuests, cmbGuests, btnLoadSelectedGuest, btnIdentitySearch"
}

$methods = @'

        // =====================================================================
        // IDENTITY / PASSPORT SEARCH FORM2 RESULT MAPPING
        // =====================================================================
        private void btnIdentitySearch_Click(object sender, EventArgs e)
        {
            using (var popup = new Form2(_connectionString, txtPassportNumber.Text))
            {
                if (popup.ShowDialog(this) == DialogResult.OK && popup.SelectedGuest != null)
                    ApplyIdentitySearchResultToForm(popup.SelectedGuest);
            }
        }

        private void ApplyIdentitySearchResultToForm(IdentityGuestSearchResult guest)
        {
            if (guest == null)
                return;

            txtTitle.Text = NormalizeIdentitySearchTitle(guest.GuestTitle);
            txtFirstName.Text = guest.FirstName;
            txtMiddleName.Text = guest.MiddleName;
            txtLastName.Text = guest.LastName;
            txtGstCod.Text = guest.GuestCode;
            txtAddress.Text = guest.GuestAddress;
            txtCountry.Text = guest.GuestCountry;
            txtCity.Text = guest.GuestCity;
            txtState.Text = guest.GuestState;
            txtZip.Text = guest.GuestZip;
            txtPhone.Text = guest.GuestTelephone;
            txtMobile.Text = guest.GuestMobile;
            txtNationality.Text = guest.Nationality;
            txtPassportNumber.Text = guest.Passport;
            txtEmail.Text = guest.GuestEmail;

            if (!string.IsNullOrWhiteSpace(guest.WorkPermit))
                txtIdNumber.Text = guest.WorkPermit;

            if (!string.IsNullOrWhiteSpace(guest.Passport))
                txtDocumentType.Text = "Passport";

            if (guest.DateOfBirth.HasValue)
            {
                chkBirthDate.Checked = true;
                dtBirthDate.Value = guest.DateOfBirth.Value;
            }
            else
            {
                chkBirthDate.Checked = false;
            }

            if (guest.PassportIssueDate.HasValue)
            {
                chkIssueDate.Checked = true;
                dtIssueDate.Value = guest.PassportIssueDate.Value;
            }
            else
            {
                chkIssueDate.Checked = false;
            }

            string gender = (guest.Gender ?? string.Empty).Trim();
            if (gender.Equals("MALE", StringComparison.OrdinalIgnoreCase))
                SelectGender(1);
            else if (gender.Equals("FEMALE", StringComparison.OrdinalIgnoreCase))
                SelectGender(2);

            SetDetailsVisible(true);
            SetMsg("Guest data loaded from identity search.");
        }

        private static string NormalizeIdentitySearchTitle(string value)
        {
            string title = (value ?? string.Empty).Trim().Trim('.');

            if (title.Equals("Mr", StringComparison.OrdinalIgnoreCase))
                return "Mr";

            if (title.Equals("Ms", StringComparison.OrdinalIgnoreCase))
                return "Ms";

            if (title.Equals("Miss", StringComparison.OrdinalIgnoreCase))
                return "Miss";

            return "Mr";
        }

'@

if ($form -notmatch "ApplyIdentitySearchResultToForm\(IdentityGuestSearchResult guest\)") {
    $marker = "        // INNER CLASS"
    if ($form.Contains($marker)) {
        $form = $form.Replace($marker, $methods + $marker)
    } else {
        $lastBrace = $form.LastIndexOf("    }")
        if ($lastBrace -lt 0) { throw "Could not find Form1 class closing brace" }
        $form = $form.Insert($lastBrace, $methods)
    }
}

# Remove older runtime button method if it was injected by the previous package.
$form = [regex]::Replace(
    $form,
    "\r?\n\s*// =====================================================================\r?\n\s*// IDENTITY / PASSPORT SEARCH BUTTON AND FORM2 RESULT MAPPING\r?\n\s*// =====================================================================\r?\n\s*private void InitializeIdentitySearchButton\(\).*?private static string NormalizeIdentitySearchTitle\(string value\)\s*\{.*?\r?\n\s*\}\r?\n",
    "`r`n",
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)

Set-Content $formPath $form -Encoding UTF8

# --------------------------------------------------------------------
# Resource duplicate protection
# --------------------------------------------------------------------
Get-ChildItem $ProjectDir -Filter "Form1.*.resx" -ErrorAction SilentlyContinue | Remove-Item -Force

if (Test-Path $csprojPath) {
    $csproj = Get-Content $csprojPath -Raw -Encoding UTF8
    if ($csproj -notmatch "Form1\.\*\.resx") {
        $csproj = $csproj -replace "`r?`n</Project>", "`r`n`r`n  <ItemGroup>`r`n    <EmbeddedResource Remove=`"Form1.*.resx`" />`r`n  </ItemGroup>`r`n`r`n</Project>"
        Set-Content $csprojPath $csproj -Encoding UTF8
    }
}

Write-Host "Form2 designer identity search patch applied successfully."
