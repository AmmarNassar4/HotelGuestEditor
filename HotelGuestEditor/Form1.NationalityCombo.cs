using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HotelGuestEditor
{
    public partial class Form1
    {
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
            btnSave_Click(sender, e);
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
    }
}
