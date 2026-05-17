using System;
using System.Windows.Forms;

namespace HotelGuestEditor
{
    public partial class Form1
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == Keys.Enter || keyData == Keys.Return) && IsNationalityComboFocused())
            {
                CommitNationalityEnterSelection();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool IsNationalityComboFocused()
        {
            return _cmbNationality != null &&
                   (_cmbNationality.Focused || _cmbNationality.ContainsFocus || ActiveControl == _cmbNationality);
        }

        private void CommitNationalityEnterSelection()
        {
            if (_cmbNationality == null)
                return;

            string value = _cmbNationality.Text?.Trim() ?? string.Empty;

            // If AutoComplete selected a suffix, keep only what the user actually typed.
            // If the whole text is selected, keep the whole value instead of treating it as empty.
            if (_cmbNationality.SelectionLength > 0 && _cmbNationality.SelectionStart > 0)
            {
                int selectedStart = _cmbNationality.SelectionStart;
                if (selectedStart < value.Length)
                    value = value.Substring(0, selectedStart).Trim();
            }

            if (string.IsNullOrWhiteSpace(value))
                value = _cmbNationality.Text?.Trim() ?? string.Empty;

            NormalizeNationalitySelection(value);

            _cmbNationality.DroppedDown = false;
            _cmbNationality.SelectionStart = _cmbNationality.Text.Length;
            _cmbNationality.SelectionLength = 0;
        }
    }
}
