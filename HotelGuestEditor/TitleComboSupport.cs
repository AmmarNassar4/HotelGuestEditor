using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HotelGuestEditor
{
    public partial class Form1
    {
        private ComboBox _cmbTitle;
        private static readonly string[] AllowedGuestTitles = { "Mr", "Ms", "Miss" };

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitializeTitleCombo();
        }

        private void InitializeTitleCombo()
        {
            if (_cmbTitle != null || txtTitle == null)
                return;

            _cmbTitle = new ComboBox
            {
                Name = "cmbTitle",
                Location = txtTitle.Location,
                Size = new Size(txtTitle.Width, txtTitle.Height),
                TabIndex = txtTitle.TabIndex,
                Visible = txtTitle.Visible,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cmbTitle.Items.AddRange(AllowedGuestTitles.Cast<object>().ToArray());

            Controls.Add(_cmbTitle);
            _cmbTitle.BringToFront();

            txtTitle.VisibleChanged += (_, __) => SyncTitleComboVisibility();
            txtTitle.EnabledChanged += (_, __) => SyncTitleComboVisibility();
            txtTitle.ReadOnlyChanged += (_, __) => SyncTitleComboVisibility();
            txtTitle.TextChanged += (_, __) => SyncTitleComboFromTextBox();
            _cmbTitle.SelectedIndexChanged += (_, __) => SyncTitleTextBoxFromCombo();

            SyncTitleComboFromTextBox();
            SyncTitleComboVisibility();
        }

        private void SyncTitleComboVisibility()
        {
            if (_cmbTitle == null || txtTitle == null)
                return;

            bool shouldShow = txtTitle.Visible;
            txtTitle.Visible = false;

            _cmbTitle.Visible = shouldShow;
            _cmbTitle.Enabled = txtTitle.Enabled && !txtTitle.ReadOnly;
            _cmbTitle.Location = txtTitle.Location;
            _cmbTitle.Size = txtTitle.Size;
        }

        private void SyncTitleComboFromTextBox()
        {
            if (_cmbTitle == null || txtTitle == null)
                return;

            string normalizedTitle = NormalizeGuestTitle(txtTitle.Text);

            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                _cmbTitle.SelectedIndex = -1;
                if (!string.IsNullOrWhiteSpace(txtTitle.Text))
                    txtTitle.Text = string.Empty;
                return;
            }

            if (!string.Equals(txtTitle.Text, normalizedTitle, StringComparison.Ordinal))
                txtTitle.Text = normalizedTitle;

            int index = _cmbTitle.Items.IndexOf(normalizedTitle);
            if (index >= 0 && _cmbTitle.SelectedIndex != index)
                _cmbTitle.SelectedIndex = index;
        }

        private void SyncTitleTextBoxFromCombo()
        {
            if (_cmbTitle == null || txtTitle == null)
                return;

            string selectedTitle = _cmbTitle.SelectedItem?.ToString() ?? string.Empty;
            if (!string.Equals(txtTitle.Text, selectedTitle, StringComparison.Ordinal))
                txtTitle.Text = selectedTitle;
        }

        private static string NormalizeGuestTitle(string value)
        {
            string title = (value ?? string.Empty).Trim().Trim('.');

            if (string.Equals(title, "Mr", StringComparison.OrdinalIgnoreCase))
                return "Mr";

            if (string.Equals(title, "Ms", StringComparison.OrdinalIgnoreCase))
                return "Ms";

            if (string.Equals(title, "Miss", StringComparison.OrdinalIgnoreCase))
                return "Miss";

            return string.Empty;
        }
    }
}
