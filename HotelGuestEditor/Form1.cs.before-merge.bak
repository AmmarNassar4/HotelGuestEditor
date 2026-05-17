using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SortOrder = System.Windows.Forms.SortOrder;
using System.Net.Mail;

namespace HotelGuestEditor
{
    public partial class Form1 : Form
    {
        // ─── SQL ──────────────────────────────────────────────────────────────
        private readonly string _connectionString =
            "Server=192.168.8.60;Database=RAMADA_WYNDHAM;User Id=sa2;Password=Qaz123456.;Encrypt=false;";

        // ─── Passport scanner ─────────────────────────────────────────────────
        private readonly RegulaPassportScanService _regulaService = new RegulaPassportScanService();

        // ─── GuestGate / SignalR ──────────────────────────────────────────────
        private readonly AppSettings _settings = AppSettings.Load();
        private string DefaultBaseUrl => _settings.BaseUrl;
        private string FixedKid => _settings.KioskId;
        private string FixedTemplateId => _settings.TemplateId;
        private string KioskDisplayName => _settings.KioskName;

        private readonly HttpClient _http = new HttpClient();
        private HubConnection _hub;
        private int _activeSessionId = 0;

        private Dictionary<string, Control> _fixedControls;
        private Dictionary<string, Label> _fixedLabels;

        // ─── Arrival reservations search ─────────────────────────────────────
        private Control[] _editorControls = Array.Empty<Control>();
        private Control[] _legacyLookupControls = Array.Empty<Control>();
        private readonly BindingSource _reservationsBindingSource = new BindingSource();
        private string _reservationSortColumn = "FullName";
        private System.ComponentModel.ListSortDirection _reservationSortDirection = System.ComponentModel.ListSortDirection.Ascending;

        // ─── Passport scanner standby ───────────────────────────────────────
        private CancellationTokenSource _passportStandbyCts;
        private bool _passportStandbyRunning;
        private readonly SemaphoreSlim _passportScanLock = new SemaphoreSlim(1, 1);

        // =====================================================================
        public Form1()
        {
            InitializeComponent();
            InitializeArrivalSearchState();
            InitializeGenderCombo();
            SetMsg("Ready");

            _fixedControls = BuildControlMap();
            _fixedLabels = BuildLabelMap();

            // Start / Stop buttons
            button3.Click += async (_, __) => await StartSessionAsync();
            button2.Click += async (_, __) => await StopSessionAsync();

            // Bootstrap GuestGate hub + template rules on form load
            this.Shown += async (_, __) => await BootstrapAsync();
        }

        // =====================================================================
        // ARRIVAL SEARCH UI  (controls are declared in Form1.Designer.cs)
        // =====================================================================
        private void InitializeArrivalSearchState()
        {
            _editorControls = new Control[]
            {
                lblResNub, txtResNub, btnScanPassport, btnPassportStandby,
                lblGstCod, txtGstCod,
                lblSrlNub, txtSrlNub,
                lblSubSrl, txtSubSrl,
                lblTitle, txtTitle,
                lblFirstName, txtFirstName,
                lblMiddleName, txtMiddleName,
                lblLastName, txtLastName,
                lblAddress, txtAddress,
                lblCity, txtCity,
                lblState, txtState,
                lblCountry, txtCountry,
                lblZip, txtZip,
                lblPhone, txtPhone,
                lblMobile, txtMobile,
                lblNationality, txtNationality,
                lblEmail, txtEmail,
                lblGender, cmbGender,
                lblGstn, txtGstn,
                lblDocumentType, txtDocumentType,
                lblPassportNumber, txtPassportNumber,
                lblPassportIssuePlace, txtPassportIssuePlace,
                lblIdType, txtIdType,
                lblIdNumber, txtIdNumber,
                lblBirthDate, chkBirthDate, dtBirthDate,
                lblIssueDate, chkIssueDate, dtIssueDate,
                lblExpiryDate, chkExpiryDate, dtExpiryDate,
                btnSave, button3, button2
            };

            // Old reservation-number lookup controls are kept for compatibility
            // with their existing handlers, but the new sortable DataGridView list
            // is the active lookup UI.
            _legacyLookupControls = new Control[]
            {
                btnLoadGuests, lblGuests, cmbGuests, btnLoadSelectedGuest
            };

            dtArrivalSearchDate.Value = DateTime.Today;
            txtResNub.ReadOnly = true;

            button3.Text = "Start";
            button2.Text = "Stop";
            button2.Enabled = false;

            SetDetailsVisible(false);
            SetGuestSelectorVisible(false);
        }

        private void SetDetailsVisible(bool visible)
        {
            // Do not stop passport standby here.
            // Standby must keep running while the receptionist changes reservations/guests.
            // It should stop only from Stop Standby or when the form is closed.

            if (_editorControls != null)
            {
                foreach (Control control in _editorControls)
                {
                    if (control != null)
                        control.Visible = visible;
                }
            }

            if (_legacyLookupControls != null)
            {
                foreach (Control control in _legacyLookupControls)
                {
                    if (control != null)
                        control.Visible = false;
                }
            }
        }

        private void SetGuestSelectorVisible(bool visible)
        {
            // After selecting a reservation from the arrival list, keep the old
            // guest selector visible so the receptionist can choose each guest
            // in that reservation separately.
            lblResNub.Visible = visible;
            txtResNub.Visible = visible;
            lblGuests.Visible = visible;
            cmbGuests.Visible = visible;
            btnLoadSelectedGuest.Visible = visible;

            // The reservation is selected from the DataGridView, so the old
            // manual Load Guests button is not needed in the new flow.
            btnLoadGuests.Visible = false;

            if (!visible)
            {
                cmbGuests.DataSource = null;
                cmbGuests.Items.Clear();
            }
        }

        private void btnSearchReservations_Click(object sender, EventArgs e)
        {
            try
            {
                LoadReservationsForArrivalDate();
            }
            catch (Exception ex)
            {
                SetMsg("Search failed: " + ex.Message);
                MessageBox.Show("Search failed: " + ex.Message);
            }
        }

        private void LoadReservationsForArrivalDate()
        {
            int arrDate = ToYyyyMmDdInt(dtArrivalSearchDate.Value.Date);

            string sql = @";WITH LastReservationLine AS
(
    SELECT
        R1.RESNUB,
        R1.SRLNUB,
        R1.SUBSRL,
        R1.UPDDAT,
        R1.UPDTIM,
        R1.ARRDAT,

        CASE
            WHEN
                ISNULL(R1.SGLBKD, 0) + ISNULL(R1.SGLPRV, 0) + ISNULL(R1.SGLCNF, 0) + ISNULL(R1.SGLWLS, 0) + ISNULL(R1.SGLREF, 0) + ISNULL(R1.SGLCMP, 0) +
                ISNULL(R1.DBLBKD, 0) + ISNULL(R1.DBLPRV, 0) + ISNULL(R1.DBLCNF, 0) + ISNULL(R1.DBLWLS, 0) + ISNULL(R1.DBLREF, 0) + ISNULL(R1.DBLCMP, 0) +
                ISNULL(R1.TPLBKD, 0) + ISNULL(R1.TPLPRV, 0) + ISNULL(R1.TPLCNF, 0) + ISNULL(R1.TPLWLS, 0) + ISNULL(R1.TPLREF, 0) + ISNULL(R1.TPLCMP, 0) +
                ISNULL(R1.QUDBKD, 0) + ISNULL(R1.QUDPRV, 0) + ISNULL(R1.QUDCNF, 0) + ISNULL(R1.QUDWLS, 0) + ISNULL(R1.QUDREF, 0) + ISNULL(R1.QUDCMP, 0) +
                ISNULL(R1.ADTPAX, 0) + ISNULL(R1.CHDPAX, 0) + ISNULL(R1.INFPAX, 0) + ISNULL(R1.EXBPAX, 0) + ISNULL(R1.CMPPAX, 0)
                = 0
            THEN 1
            ELSE 0
        END AS IsLastLineZero,

        ROW_NUMBER() OVER
        (
            PARTITION BY R1.RESNUB
            ORDER BY
                R1.UPDDAT DESC,
                R1.UPDTIM DESC,
                R1.SRLNUB DESC,
                R1.SUBSRL DESC
        ) AS LastLineRowNo
    FROM PMS.FMR01TBL R1
    WHERE R1.ARRDAT = @ARRDAT
),
ActiveReservation AS
(
    SELECT
        RESNUB,
        UPDDAT AS LastUPDDAT,
        UPDTIM AS LastUPDTIM
    FROM LastReservationLine
    WHERE LastLineRowNo = 1
      AND IsLastLineZero = 0
),
ReservationGuests AS
(
    SELECT
        CAST(R1.RESNUB AS BIGINT) AS RESNUB,
        CAST(R1.SRLNUB AS INT) AS SRLNUB,
        CAST(R1.SUBSRL AS INT) AS SUBSRL,

        AR.LastUPDDAT,
        AR.LastUPDTIM,

        LTRIM(RTRIM(CONCAT(
            NULLIF(LTRIM(RTRIM(R2.GSTTIT)), ''), ' ',
            NULLIF(LTRIM(RTRIM(R2.FSTNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(R2.MIDNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(R2.LSTNAM)), '')
        ))) AS FullName,

        CASE
            WHEN NULLIF(LTRIM(RTRIM(CONCAT(
                NULLIF(LTRIM(RTRIM(R2.GSTTIT)), ''), ' ',
                NULLIF(LTRIM(RTRIM(R2.FSTNAM)), ''), ' ',
                NULLIF(LTRIM(RTRIM(R2.MIDNAM)), ''), ' ',
                NULLIF(LTRIM(RTRIM(R2.LSTNAM)), '')
            ))), '') IS NULL THEN 1
            ELSE 0
        END AS NoNameSort,

        ROW_NUMBER() OVER
        (
            PARTITION BY R1.RESNUB
            ORDER BY
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(CONCAT(
                        NULLIF(LTRIM(RTRIM(R2.GSTTIT)), ''), ' ',
                        NULLIF(LTRIM(RTRIM(R2.FSTNAM)), ''), ' ',
                        NULLIF(LTRIM(RTRIM(R2.MIDNAM)), ''), ' ',
                        NULLIF(LTRIM(RTRIM(R2.LSTNAM)), '')
                    ))), '') IS NULL THEN 1
                    ELSE 0
                END,
                R1.SRLNUB,
                R1.SUBSRL
        ) AS RowNo
    FROM PMS.FMR01TBL R1
    INNER JOIN ActiveReservation AR
        ON AR.RESNUB = R1.RESNUB
    LEFT JOIN PMS.FMR02TBL R2
        ON R2.RESNUB = R1.RESNUB
       AND R2.SRLNUB = R1.SRLNUB
       AND R2.SUBSRL = R1.SUBSRL
    WHERE R1.ARRDAT = @ARRDAT
      AND NOT EXISTS
      (
          SELECT 1
          FROM PMS.FMOCCTBL O
          WHERE O.RESNUB = R1.RESNUB
      )
),
ReservationList AS
(
    SELECT
        RESNUB,
        SRLNUB,
        SUBSRL,
        FullName,
        NoNameSort,
        LastUPDDAT,
        LastUPDTIM
    FROM ReservationGuests
    WHERE RowNo = 1
)
SELECT
    RL.RESNUB,
    RL.SRLNUB,
    RL.SUBSRL,
    RL.FullName,
    RL.NoNameSort,
    COALESCE(
        NULLIF(LTRIM(RTRIM(R0.COMNAM)), ''),
        NULLIF(LTRIM(RTRIM(R0.BUSSOR)), ''),
        NULLIF(LTRIM(RTRIM(R0.RESMOD)), ''),
        ''
    ) AS ReservationSource,
    (
        SELECT COUNT(1)
        FROM PMS.FMR02TBL G
        WHERE G.RESNUB = RL.RESNUB
    ) AS GuestCount
FROM ReservationList RL
LEFT JOIN PMS.FMR00TBL R0
    ON R0.RESNUB = RL.RESNUB
ORDER BY
    RL.NoNameSort,
    RL.FullName,
    RL.RESNUB;";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ARRDAT", arrDate);
                con.Open();

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                _reservationsBindingSource.DataSource = dt;
                dgvReservations.DataSource = _reservationsBindingSource;

                if (txtReservationNameFilter != null)
                    txtReservationNameFilter.Clear();

                _reservationsBindingSource.Filter = string.Empty;

                ConfigureReservationsGrid();
                ApplyReservationsSort("FullName", System.ComponentModel.ListSortDirection.Ascending);
                ClearForm();
                SetDetailsVisible(false);
                SetGuestSelectorVisible(false);

                if (dt.Rows.Count == 0)
                {
                    SetMsg("No reservations found for the selected arrival date.");
                    MessageBox.Show("No reservations found for the selected arrival date.");
                    return;
                }

                SetMsg($"{dt.Rows.Count} reservation(s) found. Double-click a reservation to load its guest list.");
            }
        }

        private void txtReservationNameFilter_TextChanged(object sender, EventArgs e)
        {
            ApplyReservationNameFilter();
        }

        private void ApplyReservationNameFilter()
        {
            if (_reservationsBindingSource == null || _reservationsBindingSource.DataSource == null)
                return;

            DataTable table = _reservationsBindingSource.DataSource as DataTable;
            if (table == null || !table.Columns.Contains("FullName"))
                return;

            string filterText = txtReservationNameFilter?.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(filterText))
            {
                _reservationsBindingSource.Filter = string.Empty;
                ApplyReservationsSort(_reservationSortColumn, _reservationSortDirection);
                SetMsg($"{table.Rows.Count} reservation(s) found.");
                return;
            }

            string escaped = EscapeBindingSourceFilterValue(filterText);
            _reservationsBindingSource.Filter = $"Convert(FullName, 'System.String') LIKE '%{escaped}%'";
            ApplyReservationsSort(_reservationSortColumn, _reservationSortDirection);
            SetMsg($"{_reservationsBindingSource.Count} reservation(s) match name search.");
        }

        private static string EscapeBindingSourceFilterValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            return value
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("]", "[]]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");
        }

        private void ConfigureReservationsGrid()
        {
            if (dgvReservations == null || dgvReservations.Columns.Count == 0) return;

            foreach (DataGridViewColumn column in dgvReservations.Columns)
                column.SortMode = DataGridViewColumnSortMode.Programmatic;

            ConfigureGridColumn("RESNUB", "Reservation No", true, 130);
            ConfigureGridColumn("SRLNUB", "Reservation Serial", false, 90);
            ConfigureGridColumn("SUBSRL", "Guest Serial", false, 90);
            ConfigureGridColumn("NoNameSort", "No Name Sort", false, 60);
            ConfigureGridColumn("FullName", "Primary Guest", true, 360);
            ConfigureGridColumn("ReservationSource", "Reservation Source", true, 220);
            ConfigureGridColumn("GuestCount", "Guests", true, 80);
        }

        private void ConfigureGridColumn(string columnName, string headerText, bool visible, int width)
        {
            if (!dgvReservations.Columns.Contains(columnName)) return;

            DataGridViewColumn column = dgvReservations.Columns[columnName];
            column.HeaderText = headerText;
            column.Visible = visible;
            column.Width = width;
            column.SortMode = DataGridViewColumnSortMode.Programmatic;
        }

        private void dgvReservations_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || dgvReservations.Columns.Count == 0) return;

            DataGridViewColumn column = dgvReservations.Columns[e.ColumnIndex];
            if (column == null || !column.Visible) return;

            string columnName = string.IsNullOrWhiteSpace(column.DataPropertyName)
                ? column.Name
                : column.DataPropertyName;

            System.ComponentModel.ListSortDirection direction = System.ComponentModel.ListSortDirection.Ascending;
            if (string.Equals(_reservationSortColumn, columnName, StringComparison.OrdinalIgnoreCase) &&
                _reservationSortDirection == System.ComponentModel.ListSortDirection.Ascending)
            {
                direction = System.ComponentModel.ListSortDirection.Descending;
            }

            ApplyReservationsSort(columnName, direction);
        }

        private void ApplyReservationsSort(string columnName, System.ComponentModel.ListSortDirection direction)
        {
            if (_reservationsBindingSource == null || _reservationsBindingSource.DataSource == null) return;

            DataTable table = _reservationsBindingSource.DataSource as DataTable;
            if (table == null) return;

            if (string.IsNullOrWhiteSpace(columnName) || !table.Columns.Contains(columnName) ||
                string.Equals(columnName, "NoNameSort", StringComparison.OrdinalIgnoreCase))
            {
                columnName = "FullName";
            }

            string directionText = direction == System.ComponentModel.ListSortDirection.Descending ? "DESC" : "ASC";
            var sortParts = new List<string> { "NoNameSort ASC", columnName + " " + directionText };

            if (!string.Equals(columnName, "FullName", StringComparison.OrdinalIgnoreCase))
                sortParts.Add("FullName ASC");

            AddReservationSortFallback(sortParts, columnName, "RESNUB");
            AddReservationSortFallback(sortParts, columnName, "SRLNUB");
            AddReservationSortFallback(sortParts, columnName, "SUBSRL");

            _reservationSortColumn = columnName;
            _reservationSortDirection = direction;
            _reservationsBindingSource.Sort = string.Join(", ", sortParts);
            UpdateReservationsSortGlyph(columnName, direction);
        }

        private void AddReservationSortFallback(List<string> sortParts, string mainColumnName, string fallbackColumnName)
        {
            if (string.Equals(mainColumnName, fallbackColumnName, StringComparison.OrdinalIgnoreCase)) return;
            sortParts.Add(fallbackColumnName + " ASC");
        }

        private void UpdateReservationsSortGlyph(string columnName, System.ComponentModel.ListSortDirection direction)
        {
            if (dgvReservations == null || dgvReservations.Columns.Count == 0) return;

            foreach (DataGridViewColumn column in dgvReservations.Columns)
                column.HeaderCell.SortGlyphDirection = SortOrder.None;

            if (!dgvReservations.Columns.Contains(columnName)) return;

            dgvReservations.Columns[columnName].HeaderCell.SortGlyphDirection =
                direction == System.ComponentModel.ListSortDirection.Descending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
        }

        private void dgvReservations_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            LoadReservationFromGridRow(dgvReservations.Rows[e.RowIndex]);
        }

        private void dgvReservations_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || dgvReservations.CurrentRow == null) return;

            e.Handled = true;
            LoadReservationFromGridRow(dgvReservations.CurrentRow);
        }

        private void LoadReservationFromGridRow(DataGridViewRow gridRow)
        {
            if (!TryGetReservationNumberFromGridRow(gridRow, out long resNub))
            {
                MessageBox.Show("Please select a valid reservation row.");
                return;
            }

            LoadGuestsForReservation(resNub, true);
        }

        private bool TryGetReservationNumberFromGridRow(DataGridViewRow gridRow, out long resNub)
        {
            resNub = 0;

            try
            {
                if (gridRow?.DataBoundItem is DataRowView view)
                {
                    resNub = Convert.ToInt64(view.Row["RESNUB"], CultureInfo.InvariantCulture);
                    return resNub > 0;
                }

                if (gridRow == null) return false;
                resNub = Convert.ToInt64(gridRow.Cells["RESNUB"].Value, CultureInfo.InvariantCulture);
                return resNub > 0;
            }
            catch
            {
                return false;
            }
        }

        // =====================================================================
        // BOOTSTRAP
        // =====================================================================
        private async Task BootstrapAsync()
        {
            SetMsg("Initializing...");
            await ConnectHubAsync();
            await ApplyTemplateReceptionRulesAsync();
            await LoadFormConfigPrefillAsync();
            await RefreshActiveSessionAsync();
            SetMsg("Ready. Select an arrival date and press Search.");
        }

        // =====================================================================
        // GENDER COMBO  (uses ComboBoxItem like the original)
        // =====================================================================
        private void InitializeGenderCombo()
        {
            cmbGender.Items.Clear();
            cmbGender.Items.Add(new ComboBoxItem("Male", 1));
            cmbGender.Items.Add(new ComboBoxItem("Female", 2));
            cmbGender.DisplayMember = "Text";
            cmbGender.ValueMember = "Value";
            cmbGender.SelectedIndex = -1;
        }

        private void SelectGender(int value)
        {
            for (int i = 0; i < cmbGender.Items.Count; i++)
            {
                if (cmbGender.Items[i] is ComboBoxItem item && item.Value == value)
                {
                    cmbGender.SelectedIndex = i;
                    return;
                }
            }
            cmbGender.SelectedIndex = -1;
        }

        // Overload used by GuestGate data (string "Male"/"Female"/"1"/"2")
        private void SelectGenderFromString(string value)
        {
            if (!TrySelectGenderFromString(value))
                cmbGender.SelectedIndex = -1;
        }

        private bool TrySelectGenderFromString(string value)
        {
            string v = (value ?? string.Empty).Trim();
            if (v == "1" || string.Equals(v, "Male", StringComparison.OrdinalIgnoreCase))
            {
                SelectGender(1);
                return true;
            }

            if (v == "2" || string.Equals(v, "Female", StringComparison.OrdinalIgnoreCase))
            {
                SelectGender(2);
                return true;
            }

            return false;
        }

        private int GetSelectedGenderValue()
        {
            if (cmbGender.SelectedItem is ComboBoxItem item) return item.Value;
            return 0;
        }

        // =====================================================================
        // CONTROL / LABEL MAPS  (for template rules)
        // =====================================================================
        private Dictionary<string, Control> BuildControlMap()
        {
            return new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase)
            {
                ["GuestCode"] = txtGstCod,
                ["GstCod"] = txtGstCod,
                ["ReservationSerial"] = txtSrlNub,
                ["SrlNub"] = txtSrlNub,
                ["GuestSerial"] = txtSubSrl,
                ["SubSrl"] = txtSubSrl,
                ["Title"] = txtTitle,
                ["FirstName"] = txtFirstName,
                ["MiddleName"] = txtMiddleName,
                ["LastName"] = txtLastName,
                ["Address"] = txtAddress,
                ["City"] = txtCity,
                ["State"] = txtState,
                ["Country"] = txtCountry,
                ["Zip"] = txtZip,
                ["Phone"] = txtPhone,
                ["Mobile"] = txtMobile,
                ["Nationality"] = txtNationality,
                ["Email"] = txtEmail,
                ["Gender"] = cmbGender,
                ["GSTN"] = txtGstn,
                ["Gstn"] = txtGstn,
                ["DocumentType"] = txtDocumentType,
                ["PassportNumber"] = txtPassportNumber,
                ["PassportIssuePlace"] = txtPassportIssuePlace,
                ["IdentityTypeId"] = txtIdType,
                ["IdType"] = txtIdType,
                ["IdentityNumber"] = txtIdNumber,
                ["IdNumber"] = txtIdNumber,
                ["DateOfBirth"] = dtBirthDate,
                ["BirthDate"] = dtBirthDate,
                ["IssueDate"] = dtIssueDate,
                ["ExpiryDate"] = dtExpiryDate
            };
        }

        private Dictionary<string, Label> BuildLabelMap()
        {
            return new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase)
            {
                ["GuestCode"] = lblGstCod,
                ["ReservationSerial"] = lblSrlNub,
                ["GuestSerial"] = lblSubSrl,
                ["Title"] = lblTitle,
                ["FirstName"] = lblFirstName,
                ["MiddleName"] = lblMiddleName,
                ["LastName"] = lblLastName,
                ["Address"] = lblAddress,
                ["City"] = lblCity,
                ["State"] = lblState,
                ["Country"] = lblCountry,
                ["Zip"] = lblZip,
                ["Phone"] = lblPhone,
                ["Mobile"] = lblMobile,
                ["Nationality"] = lblNationality,
                ["Email"] = lblEmail,
                ["Gender"] = lblGender,
                ["GSTN"] = lblGstn,
                ["DocumentType"] = lblDocumentType,
                ["PassportNumber"] = lblPassportNumber,
                ["PassportIssuePlace"] = lblPassportIssuePlace,
                ["IdentityTypeId"] = lblIdType,
                ["IdentityNumber"] = lblIdNumber,
                ["DateOfBirth"] = lblBirthDate,
                ["IssueDate"] = lblIssueDate,
                ["ExpiryDate"] = lblExpiryDate
            };
        }

        // =====================================================================
        // TEMPLATE RULES
        // =====================================================================
        private async Task ApplyTemplateReceptionRulesAsync()
        {
            try
            {
                string url = $"{DefaultBaseUrl.TrimEnd('/')}/admin/templates/{Uri.EscapeDataString(FixedTemplateId)}";
                string json = await _http.GetStringAsync(url);

                JObject template = JObject.Parse(json);
                JArray fields = template["fields"] as JArray ?? new JArray();

                foreach (JToken token in fields)
                {
                    JObject field = token as JObject;
                    if (field == null) continue;

                    string scope = field["scope"]?.ToString() ?? "StartForm";
                    if (!scope.Equals("StartForm", StringComparison.OrdinalIgnoreCase)) continue;

                    string key = field["key"]?.ToString() ?? "";
                    bool editable = field["reception"]?["editable"]?.ToObject<bool?>() ?? true;
                    bool required = field["reception"]?["required"]?.ToObject<bool?>() ?? false;
                    string label = field["label"]?.ToString() ?? key;

                    ApplyEditableState(key, editable);
                    ApplyRequiredMark(key, label, required);
                }
            }
            catch (Exception ex)
            {
                SetMsg("Template rules load failed: " + ex.Message);
            }
        }

        private void ApplyEditableState(string key, bool editable)
        {
            if (!_fixedControls.TryGetValue(key, out Control control)) return;

            if (control is TextBoxBase tb)
            {
                tb.ReadOnly = !editable;
                tb.BackColor = editable ? SystemColors.Window : SystemColors.Control;
            }
            else
            {
                control.Enabled = editable;
            }

            if (key.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("BirthDate", StringComparison.OrdinalIgnoreCase))
            { chkBirthDate.Enabled = editable; dtBirthDate.Enabled = editable; }
            else if (key.Equals("IssueDate", StringComparison.OrdinalIgnoreCase))
            { chkIssueDate.Enabled = editable; dtIssueDate.Enabled = editable; }
            else if (key.Equals("ExpiryDate", StringComparison.OrdinalIgnoreCase))
            { chkExpiryDate.Enabled = editable; dtExpiryDate.Enabled = editable; }
        }

        private void ApplyRequiredMark(string key, string label, bool required)
        {
            if (!_fixedLabels.TryGetValue(key, out Label lbl)) return;
            lbl.Text = required ? $"{label} *" : label;
        }

        // =====================================================================
        // PREFILL LOAD
        // =====================================================================
        private async Task LoadFormConfigPrefillAsync()
        {
            try
            {
                string url = $"{DefaultBaseUrl.TrimEnd('/')}/tablet/{Uri.EscapeDataString(FixedKid)}/form-config";
                using HttpResponseMessage resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return;

                string json = await resp.Content.ReadAsStringAsync();
                JObject cfg = JObject.Parse(json);
                JObject prefill =
                    cfg["prefill"] as JObject
                    ?? cfg.SelectToken("$..prefill") as JObject
                    ?? new JObject();

                ApplyDataToFixedForm(prefill);
            }
            catch { }
        }

        // =====================================================================
        // COLLECT PREFILL FROM FORM  (for Start session)
        // =====================================================================
        private JObject CollectPrefillFromFixedForm()
        {
            JObject obj = new JObject();

            AddIfNotEmpty(obj, "GuestCode", txtGstCod.Text);
            AddIfNotEmpty(obj, "ReservationSerial", txtSrlNub.Text);
            AddIfNotEmpty(obj, "GuestSerial", txtSubSrl.Text);
            AddIfNotEmpty(obj, "Title", txtTitle.Text);
            AddIfNotEmpty(obj, "FirstName", txtFirstName.Text);
            AddIfNotEmpty(obj, "MiddleName", txtMiddleName.Text);
            AddIfNotEmpty(obj, "LastName", txtLastName.Text);
            AddIfNotEmpty(obj, "Address", txtAddress.Text);
            AddIfNotEmpty(obj, "City", txtCity.Text);
            AddIfNotEmpty(obj, "State", txtState.Text);
            AddIfNotEmpty(obj, "Country", txtCountry.Text);
            AddIfNotEmpty(obj, "Zip", txtZip.Text);
            AddIfNotEmpty(obj, "Phone", txtPhone.Text);
            AddIfNotEmpty(obj, "Mobile", txtMobile.Text);
            AddIfNotEmpty(obj, "Nationality", txtNationality.Text);
            AddIfNotEmpty(obj, "Email", txtEmail.Text);
            AddIfNotEmpty(obj, "GSTN", txtGstn.Text);

            if (cmbGender.SelectedItem is ComboBoxItem gi)
                obj["Gender"] = gi.Text;

            AddIfNotEmpty(obj, "DocumentType", txtDocumentType.Text);
            AddIfNotEmpty(obj, "PassportNumber", txtPassportNumber.Text);
            AddIfNotEmpty(obj, "PassportIssuePlace", txtPassportIssuePlace.Text);
            AddIfNotEmpty(obj, "IdentityTypeId", txtIdType.Text);
            AddIfNotEmpty(obj, "IdentityNumber", txtIdNumber.Text);

            AddOptionalDate(obj, "DateOfBirth", chkBirthDate, dtBirthDate);
            AddOptionalDate(obj, "IssueDate", chkIssueDate, dtIssueDate);
            AddOptionalDate(obj, "ExpiryDate", chkExpiryDate, dtExpiryDate);

            return obj;
        }

        private static void AddIfNotEmpty(JObject obj, string key, string value)
        {
            string v = (value ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(v)) obj[key] = v;
        }

        private static void AddOptionalDate(JObject obj, string key, CheckBox chk, DateTimePicker dt)
        {
            if (chk.Checked) obj[key] = dt.Value.ToString("yyyy-MM-dd");
        }

        // =====================================================================
        // APPLY GUESTGATE DATA TO FORM
        // =====================================================================
        private void ApplyDataToFixedForm(JObject source)
        {
            if (source == null) return;

            JObject data =
                source["guest"] as JObject
                ?? source["prefill"] as JObject
                ?? source;

            // Data coming back from the guest/customer page must not clear existing PMS/scanner values.
            // Empty values from the guest page are ignored.
            //SetTextIfNotEmpty(txtGstCod, GetFirstValue(data, "GuestCode", "GstCod", "GSTCOD"));
            //SetTextIfNotEmpty(txtSrlNub, GetFirstValue(data, "ReservationSerial", "SrlNub", "SRLNUB"));
            //SetTextIfNotEmpty(txtSubSrl, GetFirstValue(data, "GuestSerial", "SubSrl", "SUBSRL"));
            SetTextIfNotEmpty(txtTitle, GetFirstValue(data, "Title", "GSTTIT"));
            SetTextIfNotEmpty(txtFirstName, GetFirstValue(data, "FirstName", "FSTNAM"));
            SetTextIfNotEmpty(txtMiddleName, GetFirstValue(data, "MiddleName", "MIDNAM"));
            SetTextIfNotEmpty(txtLastName, GetFirstValue(data, "LastName", "LSTNAM"));
            SetTextIfNotEmpty(txtAddress, GetFirstValue(data, "Address", "ADDRES"));
            SetTextIfNotEmpty(txtCity, GetFirstValue(data, "City", "CITYCD"));
            SetTextIfNotEmpty(txtState, GetFirstValue(data, "State", "STATCD"));
            SetTextIfNotEmpty(txtCountry, GetFirstValue(data, "Country", "COUNTY"));
            SetTextIfNotEmpty(txtZip, GetFirstValue(data, "Zip", "ZIPCOD"));
            SetTextIfNotEmpty(txtPhone, GetFirstValue(data, "Phone", "TELNUB"));
            SetTextIfNotEmpty(txtMobile, GetFirstValue(data, "Mobile", "FAXNUB"));
            SetTextIfNotEmpty(txtNationality, GetFirstValue(data, "Nationality", "NATION"));
            SetTextIfNotEmpty(txtEmail, GetFirstValue(data, "Email", "GSTEML"));
            SetTextIfNotEmpty(txtGstn, GetFirstValue(data, "GSTN", "Gstn", "FUTU04"));

            SetTextIfNotEmpty(txtDocumentType, GetFirstValue(data, "DocumentType"));
            SetTextIfNotEmpty(txtPassportNumber, GetFirstValue(data, "PassportNumber", "PASNUB"));
            SetTextIfNotEmpty(txtPassportIssuePlace, GetFirstValue(data, "PassportIssuePlace", "ISSPLA"));
            SetTextIfNotEmpty(txtIdType, GetFirstValue(data, "IdentityTypeId", "IdType"));
            SetTextIfNotEmpty(txtIdNumber, GetFirstValue(data, "IdentityNumber", "IdNumber"));

            string genderValue = GetFirstValue(data, "Gender", "FUTU03");
            if (HasGuestPageValue(genderValue) && genderValue.Trim() != "0")
                TrySelectGenderFromString(genderValue);

            SetOptionalDateFromStringIfNotEmpty(chkBirthDate, dtBirthDate, GetFirstValue(data, "DateOfBirth", "BirthDate", "DATBTH"));
            SetOptionalDateFromStringIfNotEmpty(chkIssueDate, dtIssueDate, GetFirstValue(data, "IssueDate", "ISSDAT"));
            SetOptionalDateFromStringIfNotEmpty(chkExpiryDate, dtExpiryDate, GetFirstValue(data, "ExpiryDate", "PASEXP"));
        }

        private static string GetFirstValue(JObject data, params string[] keys)
        {
            foreach (string key in keys)
            {
                JToken token = data[key];
                string value = token?.ToString();
                if (HasGuestPageValue(value))
                    return value;
            }
            return string.Empty;
        }

        private static bool HasGuestPageValue(string value)
        {
            string v = (value ?? string.Empty).Trim();
            return v.Length > 0 && !string.Equals(v, "null", StringComparison.OrdinalIgnoreCase);
        }

        private void SetOptionalDateFromStringIfNotEmpty(CheckBox chk, DateTimePicker dt, string value)
        {
            if (!HasGuestPageValue(value) || value.Trim() == "0") return;

            if (TryParseAnyDate(value, out DateTime parsed))
            {
                chk.Checked = true;
                dt.Value = parsed;
            }
        }

        private static bool TryParseAnyDate(string value, out DateTime date)
        {
            date = DateTime.Today;
            if (string.IsNullOrWhiteSpace(value)) return false;

            string[] formats =
            {
                "yyyy-MM-dd","yyyy/MM/dd","dd/MM/yyyy","MM/dd/yyyy",
                "yyyyMMdd","dd/MM/yy","MM/dd/yy","dd-MM-yy","dd-MM-yyyy"
            };
            return DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                || DateTime.TryParse(value, out date);
        }

        // =====================================================================
        // SIGNALR HUB
        // =====================================================================
        private async Task ConnectHubAsync()
        {
            try
            {
                await DisconnectHubAsync();

                string url = $"{DefaultBaseUrl.TrimEnd('/')}/hubs/guest?kid={Uri.EscapeDataString(FixedKid)}";

                _hub = new HubConnectionBuilder()
                    .WithUrl(url)
                    .WithAutomaticReconnect()
                    .Build();

                _hub.Reconnecting += _ => { BeginInvoke(() => SetMsg("Reconnecting...")); return Task.CompletedTask; };
                _hub.Reconnected += _ => { BeginInvoke(() => SetMsg("Hub reconnected.")); return Task.CompletedTask; };
                _hub.Closed += _ => { BeginInvoke(() => SetMsg("Hub closed.")); return Task.CompletedTask; };

                _hub.On<object>("sessionStarted", payload =>
                {
                    try
                    {
                        JToken token = payload as JToken ?? JToken.FromObject(payload);
                        JToken sidTok = token.SelectToken("$..sessionId");
                        if (sidTok != null && int.TryParse(sidTok.ToString(), out int sid))
                            _activeSessionId = sid;

                        BeginInvoke(() =>
                        {
                            UpdateSessionButtons();
                            SetMsg("Session started.");
                        });
                    }
                    catch { }
                });

                _hub.On<JsonElement>("sessionCompleted", je =>
                {
                    try
                    {
                        JToken token = JToken.Parse(je.GetRawText());
                        int sid = 0;
                        JToken sidTok = token.SelectToken("$..sessionId");
                        if (sidTok != null) int.TryParse(sidTok.ToString(), out sid);

                        JObject guestInline = token.SelectToken("$..guest") as JObject ?? new JObject();

                        BeginInvoke(async () =>
                        {
                            if (sid > 0)
                                await LoadSessionResultIntoFixedFormAsync(sid);
                            else
                                ApplyDataToFixedForm(guestInline);

                            _activeSessionId = 0;
                            UpdateSessionButtons();
                            SetMsg("Guest data loaded.");
                        });
                    }
                    catch (Exception ex) { BeginInvoke(() => SetMsg("sessionCompleted error: " + ex.Message)); }
                });

                await _hub.StartAsync();
                SetMsg("Hub connected.");
            }
            catch (Exception ex) { SetMsg("Connect failed: " + ex.Message); }
        }

        private async Task DisconnectHubAsync()
        {
            if (_hub == null) return;
            try { await _hub.StopAsync(); await _hub.DisposeAsync(); }
            catch { }
            finally { _hub = null; }
        }

        private async Task RefreshActiveSessionAsync()
        {
            try
            {
                string url = $"{DefaultBaseUrl.TrimEnd('/')}/api/sessions/active?kid={Uri.EscapeDataString(FixedKid)}";
                using HttpResponseMessage r = await _http.GetAsync(url);
                if (!r.IsSuccessStatusCode)
                {
                    _activeSessionId = 0;
                    return;
                }

                string json = await r.Content.ReadAsStringAsync();
                JObject o = JObject.Parse(json);
                _activeSessionId = o["sessionId"]?.ToObject<int?>() ?? 0;
            }
            catch { _activeSessionId = 0; }
            finally { UpdateSessionButtons(); }
        }

        private void UpdateSessionButtons()
        {
            if (button3 != null) button3.Enabled = _activeSessionId <= 0;
            if (button2 != null) button2.Enabled = _activeSessionId > 0;
        }

        // =====================================================================
        // START SESSION  (button3)
        // =====================================================================
        private async Task StartSessionAsync()
        {
            if (_hub == null || _hub.State != HubConnectionState.Connected)
            {
                SetMsg("Hub is offline — reconnecting...");
                await ConnectHubAsync();
                if (_hub == null || _hub.State != HubConnectionState.Connected)
                {
                    SetMsg("Cannot start: hub offline.");
                    return;
                }
            }

            try
            {
                JObject prefill = CollectPrefillFromFixedForm();

                string url =
                    $"{DefaultBaseUrl.TrimEnd('/')}/api/sessions/start" +
                    $"?kid={Uri.EscapeDataString(FixedKid)}" +
                    $"&templateId={Uri.EscapeDataString(FixedTemplateId)}";

                JObject body = new JObject { ["prefill"] = prefill };

                using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        body.ToString(Newtonsoft.Json.Formatting.None),
                        Encoding.UTF8, "application/json")
                };

                using HttpResponseMessage res = await _http.SendAsync(req);
                string payload = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    SetMsg($"Start failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                    return;
                }

                JObject o = string.IsNullOrWhiteSpace(payload) ? new JObject() : JObject.Parse(payload);
                _activeSessionId = o["sessionId"]?.ToObject<int?>() ?? 0;

                SetMsg($"Session {_activeSessionId} started.");
                UpdateSessionButtons();
            }
            catch (Exception ex) { SetMsg("Start failed: " + ex.Message); }
        }

        // =====================================================================
        // STOP SESSION  (button2)
        // =====================================================================
        private async Task StopSessionAsync()
        {
            int knownSessionId = _activeSessionId;
            await RefreshActiveSessionAsync();
            if (_activeSessionId <= 0 && knownSessionId > 0)
            {
                _activeSessionId = knownSessionId;
                UpdateSessionButtons();
            }

            if (_activeSessionId <= 0)
            {
                SetMsg("No active session to stop.");
                UpdateSessionButtons();
                return;
            }

            try
            {
                string baseUrl = DefaultBaseUrl.TrimEnd('/');
                string kidEsc = Uri.EscapeDataString(FixedKid);
                bool stopped = false;
                string errorMessage = string.Empty;

                using (HttpResponseMessage res = await _http.DeleteAsync($"{baseUrl}/api/sessions/active?kid={kidEsc}"))
                {
                    stopped = res.IsSuccessStatusCode;
                    if (!stopped) errorMessage = $"{(int)res.StatusCode} {res.ReasonPhrase}";
                }

                if (!stopped)
                {
                    using (HttpResponseMessage fallback = await _http.PostAsync($"{baseUrl}/api/sessions/cancel?kid={kidEsc}", null))
                    {
                        stopped = fallback.IsSuccessStatusCode;
                        if (!stopped) errorMessage = $"{(int)fallback.StatusCode} {fallback.ReasonPhrase}";
                    }
                }

                if (!stopped && _activeSessionId > 0)
                {
                    using (HttpResponseMessage oldRoute = await _http.PostAsync($"{baseUrl}/api/sessions/{_activeSessionId}/stop", null))
                    {
                        stopped = oldRoute.IsSuccessStatusCode;
                        if (!stopped) errorMessage = $"{(int)oldRoute.StatusCode} {oldRoute.ReasonPhrase}";
                    }
                }

                if (!stopped)
                {
                    SetMsg("Stop failed: " + errorMessage);
                    return;
                }

                SetMsg($"Session {_activeSessionId} stopped.");
                _activeSessionId = 0;
                UpdateSessionButtons();
            }
            catch (Exception ex) { SetMsg("Stop failed: " + ex.Message); }
        }

        // =====================================================================
        // LOAD SESSION RESULT
        // =====================================================================
        private async Task LoadSessionResultIntoFixedFormAsync(int sessionId)
        {
            try
            {
                string url = $"{DefaultBaseUrl.TrimEnd('/')}/api/sessions/{sessionId}/result";
                using HttpResponseMessage res = await _http.GetAsync(url);

                if (!res.IsSuccessStatusCode)
                {
                    SetMsg($"Result load failed: {(int)res.StatusCode} {res.ReasonPhrase}");
                    return;
                }

                string json = await res.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(json);
                JObject payload =
                    result["guest"] as JObject
                    ?? result["result"] as JObject
                    ?? result["data"] as JObject
                    ?? result;

                ApplyDataToFixedForm(payload);
            }
            catch (Exception ex) { SetMsg("Result load failed: " + ex.Message); }
        }

        // =====================================================================
        // ORIGINAL HANDLERS — بالظبط زي ما هم
        // =====================================================================
        private void btnLoadGuests_Click(object sender, EventArgs e)
        {
            if (!long.TryParse(txtResNub.Text.Trim(), out long resNub))
            {
                MessageBox.Show("Please enter a valid reservation number.");
                return;
            }

            LoadGuestsForReservation(resNub, true);
        }

        private bool LoadGuestsForReservation(long resNub, bool showSelector)
        {
            string sql = @"
SELECT
    CAST(SRLNUB AS INT) AS SRLNUB,
    CAST(SUBSRL AS INT) AS SUBSRL,
    CASE
        WHEN NULLIF(LTRIM(RTRIM(CONCAT(
            NULLIF(LTRIM(RTRIM(GSTTIT)), ''), ' ',
            NULLIF(LTRIM(RTRIM(FSTNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(MIDNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(LSTNAM)), '')
        ))), '') IS NULL THEN 1
        ELSE 0
    END AS NoNameSort,
    CONCAT(
        CAST(SRLNUB AS VARCHAR(10)), ' / ', CAST(SUBSRL AS VARCHAR(10)),
        ' - ',
        LTRIM(RTRIM(CONCAT(
            NULLIF(LTRIM(RTRIM(GSTTIT)), ''), ' ',
            NULLIF(LTRIM(RTRIM(FSTNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(MIDNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(LSTNAM)), '')
        )))
    ) AS GuestDisplay
FROM PMS.FMR02TBL
WHERE RESNUB = @RESNUB
ORDER BY
    CASE
        WHEN NULLIF(LTRIM(RTRIM(CONCAT(
            NULLIF(LTRIM(RTRIM(GSTTIT)), ''), ' ',
            NULLIF(LTRIM(RTRIM(FSTNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(MIDNAM)), ''), ' ',
            NULLIF(LTRIM(RTRIM(LSTNAM)), '')
        ))), '') IS NULL THEN 1
        ELSE 0
    END,
    SRLNUB,
    SUBSRL;";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@RESNUB", resNub);
                con.Open();

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                txtResNub.Text = resNub.ToString(CultureInfo.InvariantCulture);
                ClearForm();
                txtResNub.Text = resNub.ToString(CultureInfo.InvariantCulture);

                cmbGuests.DataSource = null;
                cmbGuests.DisplayMember = "GuestDisplay";
                cmbGuests.ValueMember = "SRLNUB";
                cmbGuests.DataSource = dt;

                if (dt.Rows.Count > 0)
                    cmbGuests.SelectedIndex = 0;

                if (showSelector)
                {
                    SetDetailsVisible(false);
                    SetGuestSelectorVisible(true);
                }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No guests found for this reservation.");
                    SetMsg($"Reservation {resNub} has no guests.");
                    return false;
                }

                string message = dt.Rows.Count == 1
                    ? $"Reservation {resNub}: one guest found. Press Load Selected Guest to open it."
                    : $"Reservation {resNub}: {dt.Rows.Count} guests found. Select a guest from the list, then press Load Selected Guest.";

                SetMsg(message);
                return true;
            }
        }

        private void btnLoadSelectedGuest_Click(object sender, EventArgs e)
        {
            if (!long.TryParse(txtResNub.Text.Trim(), out long resNub))
            {
                MessageBox.Show("Please enter a valid reservation number.");
                return;
            }

            if (cmbGuests.SelectedItem == null)
            {
                MessageBox.Show("Please load guests and select one guest first.");
                return;
            }

            var row = ((DataRowView)cmbGuests.SelectedItem).Row;
            int srlNub = Convert.ToInt32(row["SRLNUB"]);
            int subSrl = Convert.ToInt32(row["SUBSRL"]);

            LoadGuestDetails(resNub, srlNub, subSrl);
        }

        private void LoadGuestDetails(long resNub, int srlNub, int subSrl)
        {
            txtResNub.Text = resNub.ToString(CultureInfo.InvariantCulture);

            string sql = @"
SELECT TOP 1
    R2.GSTCOD, R2.SRLNUB, R2.SUBSRL,
    R2.GSTTIT, R2.FSTNAM, R2.MIDNAM, R2.LSTNAM,
    R2.ADDRES, R2.CITYCD, R2.STATCD, R2.COUNTY, R2.ZIPCOD,
    R2.TELNUB, R2.FAXNUB, R2.NATION, R2.FUTU03,
    R3.DATBTH, R3.FUTU04 AS GSTN,
    R3.PASNUB, R3.ISSDAT, R3.PASEXP, R3.ISSPLA, R3.FUTU05,
    R13.GSTEML
FROM PMS.FMR02TBL R2
LEFT JOIN PMS.FMR03TBL R3
    ON R3.RESNUB = R2.RESNUB AND R3.SRLNUB = R2.SRLNUB AND R3.SUBSRL = R2.SUBSRL
LEFT JOIN PMS.FMR13TBL R13
    ON R13.RESNUB = R2.RESNUB AND R13.SRLNUB = R2.SRLNUB AND R13.SUBSRL = R2.SUBSRL
WHERE R2.RESNUB = @RESNUB AND R2.SRLNUB = @SRLNUB AND R2.SUBSRL = @SUBSRL;";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@RESNUB", resNub);
                cmd.Parameters.AddWithValue("@SRLNUB", srlNub);
                cmd.Parameters.AddWithValue("@SUBSRL", subSrl);
                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        MessageBox.Show("Guest details not found.");
                        ClearForm();
                        SetDetailsVisible(false);
                        return;
                    }

                    txtGstCod.Text = reader["GSTCOD"] == DBNull.Value ? "" : reader["GSTCOD"].ToString();
                    txtSrlNub.Text = reader["SRLNUB"].ToString();
                    txtSubSrl.Text = reader["SUBSRL"].ToString();
                    txtTitle.Text = reader["GSTTIT"] == DBNull.Value ? "" : reader["GSTTIT"].ToString();
                    txtFirstName.Text = reader["FSTNAM"] == DBNull.Value ? "" : reader["FSTNAM"].ToString();
                    txtMiddleName.Text = reader["MIDNAM"] == DBNull.Value ? "" : reader["MIDNAM"].ToString();
                    txtLastName.Text = reader["LSTNAM"] == DBNull.Value ? "" : reader["LSTNAM"].ToString();
                    txtAddress.Text = reader["ADDRES"] == DBNull.Value ? "" : reader["ADDRES"].ToString();
                    txtCity.Text = reader["CITYCD"] == DBNull.Value ? "" : reader["CITYCD"].ToString();
                    txtState.Text = reader["STATCD"] == DBNull.Value ? "" : reader["STATCD"].ToString();
                    txtCountry.Text = reader["COUNTY"] == DBNull.Value ? "" : reader["COUNTY"].ToString();
                    txtZip.Text = reader["ZIPCOD"] == DBNull.Value ? "" : reader["ZIPCOD"].ToString();
                    txtPhone.Text = reader["TELNUB"] == DBNull.Value ? "" : reader["TELNUB"].ToString();
                    txtMobile.Text = reader["FAXNUB"] == DBNull.Value ? "" : reader["FAXNUB"].ToString();
                    txtNationality.Text = reader["NATION"] == DBNull.Value ? "" : reader["NATION"].ToString();
                    txtGstn.Text = reader["GSTN"] == DBNull.Value ? "" : reader["GSTN"].ToString();
                    txtEmail.Text = reader["GSTEML"] == DBNull.Value ? "" : reader["GSTEML"].ToString();
                    txtPassportNumber.Text = reader["PASNUB"] == DBNull.Value ? "" : reader["PASNUB"].ToString();
                    txtPassportIssuePlace.Text = reader["ISSPLA"] == DBNull.Value ? "" : reader["ISSPLA"].ToString();
                    txtDocumentType.Text = string.Empty;

                    ParseIdentityFromFutu05(reader["FUTU05"] == DBNull.Value ? "" : reader["FUTU05"].ToString());

                    if (reader["FUTU03"] != DBNull.Value)
                        SelectGender(Convert.ToInt32(reader["FUTU03"]));
                    else
                        cmbGender.SelectedIndex = -1;

                    chkBirthDate.Checked = reader["DATBTH"] != DBNull.Value && reader["DATBTH"].ToString() != "0";
                    if (chkBirthDate.Checked)
                        dtBirthDate.Value = ParseYyyyMmDd(reader["DATBTH"].ToString());

                    chkIssueDate.Checked = reader["ISSDAT"] != DBNull.Value && reader["ISSDAT"].ToString() != "0";
                    if (chkIssueDate.Checked)
                        dtIssueDate.Value = ParseYyyyMmDd(reader["ISSDAT"].ToString());

                    chkExpiryDate.Checked = reader["PASEXP"] != DBNull.Value && reader["PASEXP"].ToString() != "0";
                    if (chkExpiryDate.Checked)
                        dtExpiryDate.Value = ParseYyyyMmDd(reader["PASEXP"].ToString());
                }
            }

            SetDetailsVisible(true);
            SetGuestSelectorVisible(true);
            SetMsg($"Reservation {resNub} / guest {srlNub}-{subSrl} loaded.");
        }

        private async void btnScanPassport_Click(object sender, EventArgs e)
        {
            if (_passportStandbyRunning)
            {
                MessageBox.Show("Passport standby is already running. Stop standby before using one-time scan.");
                return;
            }

            bool lockTaken = false;
            try
            {
                btnScanPassport.Enabled = false;
                await _passportScanLock.WaitAsync();
                lockTaken = true;

                DocumentInfo info = await _regulaService.ScanDocumentAsync(CancellationToken.None);

                if (info == null)
                {
                    MessageBox.Show("No document data was extracted.");
                    return;
                }

                ApplyScannedDocumentToForm(info);
                MessageBox.Show("Passport data applied to the form.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Scan failed: " + ex.Message);
            }
            finally
            {
                if (lockTaken) _passportScanLock.Release();
                btnScanPassport.Enabled = true;
            }
        }

        private void btnPassportStandby_Click(object sender, EventArgs e)
        {
            if (_passportStandbyRunning)
            {
                StopPassportStandby("Passport reader standby stopped.");
                return;
            }

            StartPassportStandby();
        }

        private void StartPassportStandby()
        {
            if (_passportStandbyRunning) return;

            _passportStandbyCts?.Dispose();
            _passportStandbyCts = new CancellationTokenSource();
            _passportStandbyRunning = true;

            btnPassportStandby.Text = "Stop Standby";
            btnScanPassport.Enabled = false;
            SetMsg("Passport reader standby: waiting for document.");

            _ = RunPassportStandbyLoopAsync(_passportStandbyCts.Token);
        }

        private void StopPassportStandby(string message = null)
        {
            if (!_passportStandbyRunning) return;

            _passportStandbyRunning = false;
            try { _passportStandbyCts?.Cancel(); } catch { }
            try { _regulaService.CancelCurrentScan(); } catch { }

            btnPassportStandby.Text = "Standby";
            btnScanPassport.Enabled = true;

            if (!string.IsNullOrWhiteSpace(message))
                SetMsg(message);
        }

        private async Task RunPassportStandbyLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _passportStandbyRunning)
            {
                bool lockTaken = false;
                try
                {
                    await _passportScanLock.WaitAsync(token);
                    lockTaken = true;

                    BeginInvoke(new Action(() =>
                    {
                        SetMsg("Passport reader standby: waiting for document.");
                    }));

                    DocumentInfo info = await _regulaService.ScanDocumentAsync(token, standbyMode: true);

                    if (token.IsCancellationRequested || !_passportStandbyRunning) break;

                    if (info != null)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            ApplyScannedDocumentToForm(info);
                            SetMsg("Passport data applied. Standby is still active.");
                        }));
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested && _passportStandbyRunning)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            SetMsg("Passport standby error: " + ex.Message);
                        }));
                    }
                }
                finally
                {
                    if (lockTaken) _passportScanLock.Release();
                }

                try { await Task.Delay(700, token); }
                catch (OperationCanceledException) { break; }
            }

            if (!token.IsCancellationRequested && _passportStandbyRunning)
            {
                BeginInvoke(new Action(() =>
                {
                    SetMsg("Passport standby ended unexpectedly.");
                    btnPassportStandby.Text = "Standby";
                    btnScanPassport.Enabled = true;
                }));
                _passportStandbyRunning = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!long.TryParse(txtResNub.Text.Trim(), out long resNub))
            {
                MessageBox.Show("Please enter a valid reservation number.");
                return;
            }

            if (!int.TryParse(txtSrlNub.Text.Trim(), out int srlNub) ||
                !int.TryParse(txtSubSrl.Text.Trim(), out int subSrl))
            {
                MessageBox.Show("Please load a guest first.");
                return;
            }

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        SaveFmr02(con, tran, resNub, srlNub, subSrl);
                        SaveFmr03(con, tran, resNub, srlNub, subSrl);
                        SaveFmr13(con, tran, resNub, srlNub, subSrl);

                        tran.Commit();
                        MessageBox.Show("Guest data saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show("Save failed: " + ex.Message);
                    }
                }
            }
        }

        // =====================================================================
        // SQL SAVE METHODS — بالظبط زي ما هم
        // =====================================================================
        private void SaveFmr02(SqlConnection con, SqlTransaction tran, long resNub, int srlNub, int subSrl)
        {
            string sql = @"
UPDATE PMS.FMR02TBL
SET
    GSTTIT = @GSTTIT, LSTNAM = @LSTNAM, MIDNAM = @MIDNAM, FSTNAM = @FSTNAM,
    ADDRES = @ADDRES, CITYCD = @CITYCD, STATCD = @STATCD, COUNTY = @COUNTY,
    ZIPCOD = @ZIPCOD, TELNUB = @TELNUB, FAXNUB = @FAXNUB,
    NATION = @NATION, FUTU03 = @FUTU03
WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL;";

            using (var cmd = new SqlCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@RESNUB", resNub);
                cmd.Parameters.AddWithValue("@SRLNUB", srlNub);
                cmd.Parameters.AddWithValue("@SUBSRL", subSrl);
                cmd.Parameters.AddWithValue("@GSTTIT", DbValue(txtTitle.Text));
                cmd.Parameters.AddWithValue("@LSTNAM", DbValue(txtLastName.Text));
                cmd.Parameters.AddWithValue("@MIDNAM", DbValue(txtMiddleName.Text));
                cmd.Parameters.AddWithValue("@FSTNAM", DbValue(txtFirstName.Text));
                cmd.Parameters.AddWithValue("@ADDRES", DbValue(txtAddress.Text));
                cmd.Parameters.AddWithValue("@CITYCD", DbValue(txtCity.Text));
                cmd.Parameters.AddWithValue("@STATCD", DbValue(txtState.Text));
                cmd.Parameters.AddWithValue("@COUNTY", DbValue(txtCountry.Text));
                cmd.Parameters.AddWithValue("@ZIPCOD", DbValue(txtZip.Text));
                cmd.Parameters.AddWithValue("@TELNUB", DbValue(txtPhone.Text));
                cmd.Parameters.AddWithValue("@FAXNUB", DbValue(txtMobile.Text));
                cmd.Parameters.AddWithValue("@NATION", DbValue(NormalizeNationalityCode(txtNationality.Text, null)));
                cmd.Parameters.AddWithValue("@FUTU03", GetSelectedGenderValue());
                cmd.ExecuteNonQuery();
            }
        }

        private void SaveFmr03(SqlConnection con, SqlTransaction tran, long resNub, int srlNub, int subSrl)
        {
            string sql = @"
IF EXISTS (
    SELECT 1 FROM PMS.FMR03TBL
    WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL
)
BEGIN
    UPDATE PMS.FMR03TBL
    SET DATBTH = @DATBTH, FUTU04 = @GSTN, PASNUB = @PASNUB,
        ISSDAT = @ISSDAT, PASEXP = @PASEXP, ISSPLA = @ISSPLA,
        FUTU05 = COALESCE(@FUTU05, FUTU05)
    WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL
END
ELSE
BEGIN
    INSERT INTO PMS.FMR03TBL (RESNUB, SRLNUB, SUBSRL, DATBTH, FUTU04, PASNUB, ISSDAT, PASEXP, ISSPLA, FUTU05)
    VALUES (@RESNUB, @SRLNUB, @SUBSRL, @DATBTH, @GSTN, @PASNUB, @ISSDAT, @PASEXP, @ISSPLA, @FUTU05)
END";

            using (var cmd = new SqlCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@RESNUB", resNub);
                cmd.Parameters.AddWithValue("@SRLNUB", srlNub);
                cmd.Parameters.AddWithValue("@SUBSRL", subSrl);
                cmd.Parameters.AddWithValue("@DATBTH", chkBirthDate.Checked ? ToYyyyMmDdInt(dtBirthDate.Value) : 0);
                cmd.Parameters.AddWithValue("@GSTN", DbValue(txtGstn.Text) ?? string.Empty);
                cmd.Parameters.AddWithValue("@PASNUB", DbValue(txtPassportNumber.Text) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ISSDAT", chkIssueDate.Checked ? ToYyyyMmDdInt(dtIssueDate.Value) : 0);
                cmd.Parameters.AddWithValue("@PASEXP", chkExpiryDate.Checked ? ToYyyyMmDdInt(dtExpiryDate.Value) : 0);
                cmd.Parameters.AddWithValue("@ISSPLA", DbValue(txtPassportIssuePlace.Text));

                object futu05Value = BuildFutu05();
                cmd.Parameters.AddWithValue("@FUTU05", futu05Value ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        private void SaveFmr13(SqlConnection con, SqlTransaction tran, long resNub, int srlNub, int subSrl)
        {
            string sql = @"
IF EXISTS (
    SELECT 1 FROM PMS.FMR13TBL
    WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL
)
BEGIN
    UPDATE PMS.FMR13TBL
    SET GSTEML = @GSTEML
    WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL
END
ELSE
BEGIN
    INSERT INTO PMS.FMR13TBL (RESNUB, SRLNUB, SUBSRL, GSTEML, NUMBF6, NUMBF7)
    VALUES (@RESNUB, @SRLNUB, @SUBSRL, @GSTEML, 1, 1)
END";

            using (var cmd = new SqlCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@RESNUB", resNub);

                cmd.Parameters.AddWithValue("@SRLNUB", srlNub);
                cmd.Parameters.AddWithValue("@SUBSRL", subSrl);
                cmd.Parameters.AddWithValue("@GSTEML", DbValue(txtEmail.Text));
                cmd.ExecuteNonQuery();
            }
        }

        // =====================================================================
        // PASSPORT SCAN HELPERS — 
        // =====================================================================
        private void ApplyScannedDocumentToForm(DocumentInfo info)
        {
            if (info == null) return;

            SetTextIfNotEmpty(txtDocumentType, info.DocumentType);

            if (string.Equals(info.DocumentType, "Passport", StringComparison.OrdinalIgnoreCase))
                SetTextIfNotEmpty(txtPassportNumber, info.DocumentNumber);
            else if (string.Equals(info.DocumentType, "ID Card", StringComparison.OrdinalIgnoreCase))
                SetTextIfNotEmpty(txtIdNumber, info.DocumentNumber);
            else
                SetTextIfNotEmpty(txtPassportNumber, info.DocumentNumber);

            SetTextIfNotEmpty(txtNationality, NormalizeNationalityCode(info.NationalityCode, info.Nationality));
            ApplyName(info);
            ApplyGender(info);

            if (TryParseRegulaDate(info.DateOfBirth, out DateTime birthDate))
            { chkBirthDate.Checked = true; dtBirthDate.Value = birthDate; }

            if (TryParseRegulaDate(info.IssueDate, out DateTime issueDate))
            { chkIssueDate.Checked = true; dtIssueDate.Value = issueDate; }

            if (TryParseRegulaDate(info.ExpiryDate, out DateTime expiryDate))
            { chkExpiryDate.Checked = true; dtExpiryDate.Value = expiryDate; }
        }

        private void ApplyName(DocumentInfo info)
        {
            string fullName = !string.IsNullOrWhiteSpace(info.NameEnglish)
                ? info.NameEnglish.Trim()
                : info.NameArabic?.Trim();

            if (string.IsNullOrWhiteSpace(fullName)) return;

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (parts.Count == 1)
            { SetTextIfNotEmpty(txtFirstName, parts[0]); return; }

            if (parts.Count == 2)
            { SetTextIfNotEmpty(txtFirstName, parts[0]); SetTextIfNotEmpty(txtLastName, parts[1]); return; }

            SetTextIfNotEmpty(txtFirstName, parts[0]);
            SetTextIfNotEmpty(txtLastName, parts[parts.Count - 1]);
            SetTextIfNotEmpty(txtMiddleName, string.Join(" ", parts.Skip(1).Take(parts.Count - 2)));
        }

        private void ApplyGender(DocumentInfo info)
        {
            string gender = info.Gender?.Trim();

            if (string.IsNullOrWhiteSpace(gender) && !string.IsNullOrWhiteSpace(info.GenderCode))
            {
                string code = info.GenderCode.Trim().ToUpperInvariant();
                if (code == "M") gender = "Male";
                else if (code == "F") gender = "Female";
            }

            if (string.IsNullOrWhiteSpace(gender) && !string.IsNullOrWhiteSpace(info.GenderArabic))
            {
                if (info.GenderArabic.Contains("ذكر")) gender = "Male";
                else if (info.GenderArabic.Contains("أنث")) gender = "Female";
            }

            if (gender == "Male") SelectGender(1);
            else if (gender == "Female") SelectGender(2);
        }

        // =====================================================================
        // GENERAL HELPERS 
        // =====================================================================
        private void ParseIdentityFromFutu05(string futu05)
        {
            if (string.IsNullOrWhiteSpace(futu05))
            { txtIdType.Text = string.Empty; txtIdNumber.Text = string.Empty; return; }

            string value = futu05.Trim();
            txtIdType.Text = value.Length >= 2 ? value.Substring(0, 2) : value;
            txtIdNumber.Text = value.Length > 2 ? value.Substring(2) : string.Empty;
        }

        private string BuildFutu05()
        {
            string idType = txtIdType.Text?.Trim() ?? string.Empty;
            string idNumber = txtIdNumber.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(idType) && string.IsNullOrWhiteSpace(idNumber))
                return null;

            if (idType.Length > 2) idType = idType.Substring(0, 2);

            return idType + idNumber;
        }

        private string NormalizeNationalityCode(string code, string fallbackName)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                string normalized = code.Trim().ToUpperInvariant();
                if (normalized.Length >= 3) return normalized.Substring(0, 3);
            }

            if (!string.IsNullOrWhiteSpace(fallbackName))
            {
                string normalized = fallbackName.Trim().ToUpperInvariant();
                if (normalized.Length == 3) return normalized;
            }

            return string.Empty;
        }

        private void SetTextOverride(TextBox target, string value)
        {
            target.Text = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private void SetTextIfNotEmpty(TextBox target, string value)
        {
            if (target == null || !HasGuestPageValue(value)) return;
            target.Text = value.Trim();
        }

        private object DbValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private int ToYyyyMmDdInt(DateTime date)
        {
            return int.Parse(date.ToString("yyyyMMdd"));
        }

        private DateTime ParseYyyyMmDd(string value)
        {
            string clean = value.Trim();
            int year = int.Parse(clean.Substring(0, 4));
            int month = int.Parse(clean.Substring(4, 2));
            int day = int.Parse(clean.Substring(6, 2));
            return new DateTime(year, month, day);
        }

        private bool TryParseRegulaDate(string value, out DateTime result)
        {
            string[] formats = { "dd/MM/yy", "dd/MM/yyyy", "dd-MM-yy", "dd-MM-yyyy", "yyyyMMdd" };
            return DateTime.TryParseExact(
                value?.Trim(), formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result);
        }

        private void ClearForm()
        {
            txtGstCod.Clear(); txtSrlNub.Clear(); txtSubSrl.Clear();
            txtTitle.Clear(); txtFirstName.Clear(); txtMiddleName.Clear(); txtLastName.Clear();
            txtAddress.Clear(); txtCity.Clear(); txtState.Clear(); txtCountry.Clear(); txtZip.Clear();
            txtPhone.Clear(); txtMobile.Clear(); txtNationality.Clear(); txtEmail.Clear(); txtGstn.Clear();
            txtDocumentType.Clear(); txtPassportNumber.Clear(); txtPassportIssuePlace.Clear();
            txtIdType.Clear(); txtIdNumber.Clear();
            cmbGender.SelectedIndex = -1;
            chkBirthDate.Checked = false;
            chkIssueDate.Checked = false;
            chkExpiryDate.Checked = false;
        }

        private void SetMsg(string text)
        {
            string kioskName = string.IsNullOrWhiteSpace(KioskDisplayName)
                ? FixedKid
                : KioskDisplayName;

            string prefix = $"Reservation Guest Editor — Kiosk: {kioskName}";
            Text = string.IsNullOrWhiteSpace(text)
                ? prefix
                : $"{prefix} — {text}";
        }

        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            StopPassportStandby();
            _passportStandbyCts?.Dispose();
            _regulaService.Dispose();
            await DisconnectHubAsync();
            base.OnFormClosed(e);
        }

        // =====================================================================


        // =====================================================================
        // MERGED FROM Form1.NationalityCombo.cs
        // =====================================================================
        private ComboBox _cmbNationality;
        private List<CountryInfo> _nationalityCountries = new List<CountryInfo>();
        private bool _nationalitySyncing;

        private void InitializeNationalityCombo()
        {
            if (_cmbNationality != null)
                return;

            _nationalityCountries = CountryCatalog.Load();

            _cmbNationality = new ComboBox
            {
                Name = "cmbNationality",
                Location = txtNationality.Location,
                Size = new Size(txtNationality.Width, txtNationality.Height),
                TabIndex = txtNationality.TabIndex,
                Visible = txtNationality.Visible,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.CustomSource
            };

            foreach (CountryInfo country in _nationalityCountries)
                _cmbNationality.Items.Add(country);

            var autoComplete = new AutoCompleteStringCollection();
            autoComplete.AddRange(_nationalityCountries
                .SelectMany(c => new[] { c.Name, c.Code, c.DisplayName })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
            _cmbNationality.AutoCompleteCustomSource = autoComplete;

            Controls.Add(_cmbNationality);
            _cmbNationality.BringToFront();

            txtNationality.VisibleChanged += (_, __) => SyncNationalityComboVisibility();
            txtNationality.TextChanged += (_, __) => SyncNationalityComboFromHiddenText();
            _cmbNationality.SelectedIndexChanged += (_, __) => SyncHiddenNationalityFromCombo();
            _cmbNationality.Leave += (_, __) => NormalizeNationalitySelection();
            _cmbNationality.KeyDown += cmbNationality_KeyDown;
            btnSave.MouseDown += (_, __) => NormalizeNationalitySelection();

            btnSave.Click -= btnSave_Click;
            btnSave.Click += btnSave_ClickWithNationalityNormalization;

            SyncNationalityComboFromHiddenText();
            SyncNationalityComboVisibility();
        }

        private void cmbNationality_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            string typedValue = GetNationalityTypedText();

            e.Handled = true;
            e.SuppressKeyPress = true;

            BeginInvoke(new Action(() =>
            {
                NormalizeNationalitySelection(typedValue);

                if (_cmbNationality != null)
                {
                    _cmbNationality.DroppedDown = false;
                    _cmbNationality.SelectionStart = _cmbNationality.Text.Length;
                    _cmbNationality.SelectionLength = 0;
                }
            }));
        }

        private string GetNationalityTypedText()
        {
            if (_cmbNationality == null)
                return string.Empty;

            string text = _cmbNationality.Text ?? string.Empty;

            if (_cmbNationality.SelectionLength > 0 && _cmbNationality.SelectionStart >= 0)
            {
                int selectedStart = _cmbNationality.SelectionStart;
                if (selectedStart < text.Length)
                    text = text.Substring(0, selectedStart);
            }

            return text.Trim();
        }

        private void btnSave_ClickWithNationalityNormalization(object sender, EventArgs e)
        {
            NormalizeNationalitySelection();

            if (!PrepareGuestFieldsBeforeSave())
                return;

            SaveGuestWithVisaNumber();
        }

        private void SyncNationalityComboVisibility()
        {
            if (_cmbNationality == null)
                return;

            bool shouldShow = txtNationality.Visible;
            txtNationality.Visible = false;
            _cmbNationality.Visible = shouldShow;
            _cmbNationality.Enabled = txtNationality.Enabled;
            _cmbNationality.Location = txtNationality.Location;
            _cmbNationality.Size = txtNationality.Size;
        }

        private void SyncNationalityComboFromHiddenText()
        {
            if (_cmbNationality == null || _nationalitySyncing)
                return;

            _nationalitySyncing = true;
            try
            {
                string value = txtNationality.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value))
                {
                    _cmbNationality.SelectedIndex = -1;
                    _cmbNationality.Text = string.Empty;
                    return;
                }

                CountryInfo match = FindCountry(value);
                if (match != null)
                {
                    _cmbNationality.SelectedItem = match;
                    _cmbNationality.Text = match.DisplayName;
                    txtNationality.Text = match.Code;
                }
                else
                {
                    _cmbNationality.SelectedIndex = -1;
                    _cmbNationality.Text = value;
                }
            }
            finally
            {
                _nationalitySyncing = false;
            }
        }

        private void SyncHiddenNationalityFromCombo()
        {
            if (_cmbNationality == null || _nationalitySyncing)
                return;

            if (_cmbNationality.SelectedItem is CountryInfo country)
            {
                _nationalitySyncing = true;
                try
                {
                    txtNationality.Text = country.Code;
                    _cmbNationality.Text = country.DisplayName;
                }
                finally
                {
                    _nationalitySyncing = false;
                }
            }
        }

        private void NormalizeNationalitySelection(string inputText = null)
        {
            if (_cmbNationality == null || _nationalitySyncing)
                return;

            _nationalitySyncing = true;
            try
            {
                string value = (inputText ?? _cmbNationality.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    txtNationality.Text = string.Empty;
                    _cmbNationality.SelectedIndex = -1;
                    _cmbNationality.Text = string.Empty;
                    return;
                }

                CountryInfo match = FindCountry(value);
                if (match != null)
                {
                    _cmbNationality.SelectedItem = match;
                    _cmbNationality.Text = match.DisplayName;
                    txtNationality.Text = match.Code;
                }
                else
                {
                    _cmbNationality.SelectedIndex = -1;
                    _cmbNationality.Text = value;
                    txtNationality.Text = value.Trim().ToUpperInvariant();
                }
            }
            finally
            {
                _nationalitySyncing = false;
            }
        }

        private CountryInfo FindCountry(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string text = value.Trim();
            string upper = text.ToUpperInvariant();

            CountryInfo exact = _nationalityCountries.FirstOrDefault(c =>
                string.Equals(c.Code, upper, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, text, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.DisplayName, text, StringComparison.OrdinalIgnoreCase));

            if (exact != null)
                return exact;

            string[] parts = text.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string part in parts)
            {
                CountryInfo partMatch = _nationalityCountries.FirstOrDefault(c =>
                    string.Equals(c.Code, part, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, part, StringComparison.OrdinalIgnoreCase));

                if (partMatch != null)
                    return partMatch;
            }

            return _nationalityCountries.FirstOrDefault(c =>
                c.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                c.Code.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                c.DisplayName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    


        // =====================================================================
        // MERGED FROM Form1.NationalityStartup.cs
        // =====================================================================
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeNationalityCombo();
            InitializeVisaNumberField();
        }
    


        // =====================================================================
        // MERGED FROM Form1.SaveValidation.cs
        // =====================================================================
        private bool PrepareGuestFieldsBeforeSave()
        {
            SanitizeGuestNameFields();
            return ValidateEmailBeforeSave();
        }

        private void SanitizeGuestNameFields()
        {
            SanitizeNameTextBox(txtTitle);
            SanitizeNameTextBox(txtFirstName);
            SanitizeNameTextBox(txtMiddleName);
            SanitizeNameTextBox(txtLastName);
        }

        private static void SanitizeNameTextBox(TextBox textBox)
        {
            if (textBox == null)
                return;

            textBox.Text = SanitizeNameValue(textBox.Text);
        }

        private static string SanitizeNameValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder();
            bool lastWasSpace = false;
            bool lastWasSeparator = false;

            foreach (char ch in value.Trim())
            {
                if (char.IsLetter(ch))
                {
                    builder.Append(ch);
                    lastWasSpace = false;
                    lastWasSeparator = false;
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (!lastWasSpace && builder.Length > 0)
                    {
                        builder.Append(' ');
                        lastWasSpace = true;
                        lastWasSeparator = false;
                    }
                }
                else if (ch == '.' || ch == '-')
                {
                    if (!lastWasSeparator && builder.Length > 0)
                    {
                        builder.Append(ch);
                        lastWasSpace = false;
                        lastWasSeparator = true;
                    }
                }
            }

            return builder.ToString().Trim(' ', '.', '-');
        }

        private bool ValidateEmailBeforeSave()
        {
            string email = txtEmail.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email is required before saving.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!IsValidEmailAddress(email))
            {
                MessageBox.Show("Please enter a valid email address before saving.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                txtEmail.SelectAll();
                return false;
            }

            txtEmail.Text = email;
            return true;
        }

        private static bool IsValidEmailAddress(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || email.Any(char.IsWhiteSpace))
                    return false;

                if (email.Count(ch => ch == '@') != 1)
                    return false;

                var address = new MailAddress(email);

                if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (string.IsNullOrWhiteSpace(address.Host) || !address.Host.Contains('.'))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    


        // =====================================================================
        // MERGED FROM Form1.SessionMonitor.cs
        // =====================================================================
        private readonly System.Windows.Forms.Timer _sessionMonitorTimer = new System.Windows.Forms.Timer { Interval = 10000 };
        private bool _sessionMonitorBusy;
        private bool _sessionMonitorAutoRestartEnabled;
        private bool _sessionMonitorRestartPending;
        private bool _sessionMonitorManualStopRequested;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartSessionMonitor();
        }

        private void StartSessionMonitor()
        {
            if (_sessionMonitorTimer.Enabled)
                return;

            button3.Click += (_, __) =>
            {
                _sessionMonitorManualStopRequested = false;
                _sessionMonitorAutoRestartEnabled = true;
                _sessionMonitorRestartPending = false;
            };

            button2.Click += (_, __) =>
            {
                _sessionMonitorManualStopRequested = true;
                _sessionMonitorAutoRestartEnabled = false;
                _sessionMonitorRestartPending = false;
            };

            _sessionMonitorTimer.Tick += async (_, __) => await CheckActiveSessionFromTimerAsync();
            _sessionMonitorTimer.Start();
        }

        private void StopSessionMonitor()
        {
            _sessionMonitorTimer.Stop();
            _sessionMonitorTimer.Dispose();
        }

        private async Task CheckActiveSessionFromTimerAsync()
        {
            if (_sessionMonitorBusy)
                return;

            _sessionMonitorBusy = true;

            try
            {
                int previousSessionId = _activeSessionId;

                await RefreshActiveSessionAsync();

                if (_activeSessionId > 0)
                {
                    if (!_sessionMonitorManualStopRequested)
                        _sessionMonitorAutoRestartEnabled = true;

                    _sessionMonitorRestartPending = false;
                    return;
                }

                bool activeSessionDisappeared = previousSessionId > 0 && _activeSessionId <= 0;
                if (activeSessionDisappeared && _sessionMonitorAutoRestartEnabled && !_sessionMonitorManualStopRequested)
                    _sessionMonitorRestartPending = true;

                if (_sessionMonitorRestartPending && _sessionMonitorAutoRestartEnabled && !_sessionMonitorManualStopRequested)
                {
                    SetMsg("No active session detected. Starting a new session...");
                    await StartSessionAsync();

                    if (_activeSessionId > 0)
                        _sessionMonitorRestartPending = false;
                }
            }
            catch (Exception ex)
            {
                SetMsg("Session monitor failed: " + ex.Message);
            }
            finally
            {
                _sessionMonitorBusy = false;
            }
        }
    


        // =====================================================================
        // MERGED FROM Form1.VisaNumber.cs
        // =====================================================================
        private void InitializeVisaNumberField()
        {
            ApplyVisaNumberFieldLabel();

            lblGstn.TextChanged += (_, __) => ApplyVisaNumberFieldLabel();
            btnLoadSelectedGuest.Click += (_, __) => LoadVisaNumberForCurrentGuest();
        }

        private void ApplyVisaNumberFieldLabel()
        {
            if (lblGstn == null)
                return;

            string expectedText = "Visa Number";
            if (!string.Equals(lblGstn.Text, expectedText, StringComparison.OrdinalIgnoreCase))
                lblGstn.Text = expectedText;
        }

        private void LoadVisaNumberForCurrentGuest()
        {
            if (!TryGetCurrentGuestKey(out long resNub, out int srlNub, out int subSrl))
                return;

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 VISNUB
FROM PMS.FMR03TBL
WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL;", con))
                {
                    cmd.Parameters.AddWithValue("@RESNUB", resNub);
                    cmd.Parameters.AddWithValue("@SRLNUB", srlNub);
                    cmd.Parameters.AddWithValue("@SUBSRL", subSrl);

                    con.Open();
                    object value = cmd.ExecuteScalar();
                    txtGstn.Text = value == null || value == DBNull.Value ? string.Empty : value.ToString();
                }
            }
            catch (Exception ex)
            {
                SetMsg("Visa number load failed: " + ex.Message);
            }
        }

        private bool SaveGuestWithVisaNumber()
        {
            if (!long.TryParse(txtResNub.Text.Trim(), out long resNub))
            {
                MessageBox.Show("Please enter a valid reservation number.");
                return false;
            }

            if (!int.TryParse(txtSrlNub.Text.Trim(), out int srlNub) ||
                !int.TryParse(txtSubSrl.Text.Trim(), out int subSrl))
            {
                MessageBox.Show("Please load a guest first.");
                return false;
            }

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        SaveFmr02(con, tran, resNub, srlNub, subSrl);
                        SaveFmr03WithVisaNumber(con, tran, resNub, srlNub, subSrl);
                        SaveFmr13(con, tran, resNub, srlNub, subSrl);

                        tran.Commit();
                        MessageBox.Show("Guest data saved successfully.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show("Save failed: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        private void SaveFmr03WithVisaNumber(SqlConnection con, SqlTransaction tran, long resNub, int srlNub, int subSrl)
        {
            string sql = @"
IF EXISTS (
    SELECT 1 FROM PMS.FMR03TBL
    WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL
)
BEGIN
    UPDATE PMS.FMR03TBL
    SET DATBTH = @DATBTH, VISNUB = @VISNUB, PASNUB = @PASNUB,
        ISSDAT = @ISSDAT, PASEXP = @PASEXP, ISSPLA = @ISSPLA,
        FUTU05 = COALESCE(@FUTU05, FUTU05)
    WHERE RESNUB = @RESNUB AND SRLNUB = @SRLNUB AND SUBSRL = @SUBSRL
END
ELSE
BEGIN
    INSERT INTO PMS.FMR03TBL (RESNUB, SRLNUB, SUBSRL, DATBTH, VISNUB, PASNUB, ISSDAT, PASEXP, ISSPLA, FUTU05)
    VALUES (@RESNUB, @SRLNUB, @SUBSRL, @DATBTH, @VISNUB, @PASNUB, @ISSDAT, @PASEXP, @ISSPLA, @FUTU05)
END";

            using (var cmd = new SqlCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@RESNUB", resNub);
                cmd.Parameters.AddWithValue("@SRLNUB", srlNub);
                cmd.Parameters.AddWithValue("@SUBSRL", subSrl);
                cmd.Parameters.AddWithValue("@DATBTH", chkBirthDate.Checked ? ToYyyyMmDdInt(dtBirthDate.Value) : 0);
                cmd.Parameters.AddWithValue("@VISNUB", DbValue(txtGstn.Text) ?? string.Empty);
                cmd.Parameters.AddWithValue("@PASNUB", DbValue(txtPassportNumber.Text) ?? string.Empty);
                cmd.Parameters.AddWithValue("@ISSDAT", chkIssueDate.Checked ? ToYyyyMmDdInt(dtIssueDate.Value) : 0);
                cmd.Parameters.AddWithValue("@PASEXP", chkExpiryDate.Checked ? ToYyyyMmDdInt(dtExpiryDate.Value) : 0);
                cmd.Parameters.AddWithValue("@ISSPLA", DbValue(txtPassportIssuePlace.Text));

                object futu05Value = BuildFutu05();
                cmd.Parameters.AddWithValue("@FUTU05", futu05Value ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        private bool TryGetCurrentGuestKey(out long resNub, out int srlNub, out int subSrl)
        {
            resNub = 0;
            srlNub = 0;
            subSrl = 0;

            return long.TryParse(txtResNub.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out resNub)
                && int.TryParse(txtSrlNub.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out srlNub)
                && int.TryParse(txtSubSrl.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out subSrl)
                && resNub > 0
                && srlNub > 0;
        }
    

        // INNER CLASS
        // =====================================================================
        private class ComboBoxItem
        {
            public string Text { get; }
            public int Value { get; }
            public ComboBoxItem(string text, int value) { Text = text; Value = value; }
        }

        private void txtGstCod_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbGuests_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
