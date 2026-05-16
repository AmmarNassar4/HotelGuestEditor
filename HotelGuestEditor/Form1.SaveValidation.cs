using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

namespace HotelGuestEditor
{
    public partial class Form1
    {
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
    }
}
