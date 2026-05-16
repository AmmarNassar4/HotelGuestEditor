using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace HotelGuestEditor
{
    public partial class Form1
    {
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
    }
}
