using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace HotelGuestEditor
{
    public partial class Form2 : Form
    {
        private readonly string _connectionString;
        private readonly string _identityNumber;
        private readonly bool _cancelRequested;
        private readonly BindingSource bindingSource = new BindingSource();

        public IdentityGuestSearchResult SelectedGuest { get; private set; }

        public Form2(string connectionString, string initialIdentityNumber = "")
        {
            _connectionString = connectionString ?? string.Empty;

            string identity = PromptForIdentityNumber(initialIdentityNumber);
            if (string.IsNullOrWhiteSpace(identity))
            {
                _cancelRequested = true;
            }

            _identityNumber = identity.Trim();

            InitializeComponent();
            dgvResults.DataSource = bindingSource;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_cancelRequested)
            {
                BeginInvoke(new Action(() =>
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }));
                return;
            }

            txtIdentity.Text = _identityNumber;
            LoadGuestsByIdentityNumber();
        }

        private static string PromptForIdentityNumber(string initialValue)
        {
            using (var prompt = new Form())
            using (var lbl = new Label())
            using (var txt = new TextBox())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                prompt.Text = "Identity Search";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                prompt.ShowInTaskbar = false;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;
                prompt.ClientSize = new Size(430, 125);

                lbl.Text = "Enter identity / passport number:";
                lbl.Location = new Point(15, 15);
                lbl.Size = new Size(390, 23);

                txt.Location = new Point(15, 45);
                txt.Size = new Size(395, 23);
                txt.Text = initialValue ?? string.Empty;
                txt.SelectAll();

                ok.Text = "OK";
                ok.Location = new Point(240, 82);
                ok.Size = new Size(80, 28);
                ok.DialogResult = DialogResult.OK;

                cancel.Text = "Cancel";
                cancel.Location = new Point(330, 82);
                cancel.Size = new Size(80, 28);
                cancel.DialogResult = DialogResult.Cancel;

                prompt.Controls.Add(lbl);
                prompt.Controls.Add(txt);
                prompt.Controls.Add(ok);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog() == DialogResult.OK
                    ? (txt.Text ?? string.Empty).Trim()
                    : string.Empty;
            }
        }

        private void txtIdentity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            LoadGuestsByIdentityNumber();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadGuestsByIdentityNumber();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            SelectCurrentRow();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                SelectCurrentRow();
        }

        private void dgvResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            SelectCurrentRow();
        }

        private void LoadGuestsByIdentityNumber()
        {
            string identityNumber = (txtIdentity.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                MessageBox.Show(this, "Please enter identity / passport number.", "Identity Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdentity.Focus();
                return;
            }

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(BuildSearchSql(), con))
                {
                    cmd.Parameters.AddWithValue("@PASSPORT", identityNumber);

                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }

                    bindingSource.DataSource = table;

                    if (table.Rows.Count == 0)
                    {
                        MessageBox.Show(this, "No guests found for this identity / passport number.", "Identity Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        dgvResults.Focus();
                        if (dgvResults.Rows.Count > 0)
                            dgvResults.Rows[0].Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Search failed: " + ex.Message, "Identity Search", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectCurrentRow()
        {
            if (bindingSource.Current is DataRowView view)
            {
                SelectedGuest = IdentityGuestSearchResult.FromDataRow(view.Row);
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MessageBox.Show(this, "Please select a guest row first.", "Identity Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string BuildSearchSql()
        {
            return @"
SELECT 
    COALESCE(A.GSTTIT, '') AS [Guest Title],
    COALESCE(A.FSTNAM, '') AS [First Name],
    COALESCE(A.LSTNAM, '') AS [Last Name],
    COALESCE(A.MIDNAM, '') AS [Middle Name],
 COALESCE(I.FUTU04, '') AS [Work Permit],
    COALESCE(E.GSTEML, '') AS [Guest Email],
COALESCE(A.RESTL1, '') AS [Guest Telephone],
    COALESCE(A.RESTL2, '') AS [Guest Mobile],
    COALESCE(A.GSTCOD, 0) AS [Guest Code],
    COALESCE(A.RESADD, '') AS [Guest Address],
    COALESCE(A.RESCNT, '') AS [Guest Country],
    COALESCE(A.RESCTY, '') AS [Guest City],
    COALESCE(A.RESSTA, '') AS [Guest State],
    COALESCE(A.RESZIP, '') AS [Guest Zip],

    

    COALESCE(A.COMNAM, '') AS [Company Name],
    COALESCE(B.LNGNAM, '') AS [Nationality],
    COALESCE(P.PASNUB, '') AS [Passport],

    COALESCE(
        CASE 
            WHEN A.SEXCOD <> 0 THEN
                CASE 
                    WHEN A.SEXCOD = 1 THEN 'MALE'
                    ELSE 'FEMALE'
                END
            ELSE ''
        END,
    '') AS [Gender],

    CASE 
        WHEN G.GSTBTH <> 0 
            THEN CONVERT(DATETIME, CONVERT(CHAR(8), G.GSTBTH))
        ELSE NULL
    END AS [Date Of Birth],

    CASE 
        WHEN G.DATANN <> 0 
            THEN CONVERT(DATETIME, CONVERT(CHAR(8), G.DATANN))
        ELSE NULL
    END AS [Anniversary],

    CASE 
        WHEN G.SMOKIG = 0 THEN 'YES'
        ELSE 'NO'
    END AS [Smoking],

   
    COALESCE(E.CHARF9, '') AS [Black List],

    CASE 
        WHEN P.ISSDAT <> 0 
            THEN CONVERT(DATETIME, CONVERT(CHAR(8), P.ISSDAT))
        ELSE NULL
    END AS [Passport issue date]

FROM PMS.GSMANTBL A

LEFT JOIN PMS.PR009TBL B 
    ON B.APPDAT = (
        SELECT MAX(Z.APPDAT)
        FROM PMS.PR009TBL Z
        WHERE Z.NATION = B.NATION
          AND Z.APPDAT <= A.LSTDAT
    )
   AND A.NATION = B.NATION

LEFT JOIN PMS.GSGEXTBL E 
    ON A.GSTCOD = E.GSTCOD
   AND A.MODCOD = E.MODCOD

LEFT JOIN PMS.GSPASTBL P 
    ON A.GSTCOD = P.GSTCOD
   AND A.MODCOD = P.MODCOD

LEFT JOIN PMS.GSPERTBL G 
    ON G.GSTCOD = A.GSTCOD
   AND G.MODCOD = A.MODCOD

LEFT JOIN PMS.GSIDNTBL I 
    ON I.GSTCOD = A.GSTCOD
   AND I.MODCOD = A.MODCOD

WHERE LTRIM(RTRIM(COALESCE(P.PASNUB, ''))) = @PASSPORT

ORDER BY A.GSTCOD DESC;";
        }
    }
}
