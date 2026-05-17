namespace HotelGuestEditor
{
    public partial class Form1
    {
        private bool _overlayFixDone;

        private void ApplyOverlayFixOnce()
        {
            if (_overlayFixDone) return;
            _overlayFixDone = true;

            lblTitle.VisibleChanged += (_, __) => { if (_cmbTitle != null) _cmbTitle.Visible = lblTitle.Visible; };
            lblNationality.VisibleChanged += (_, __) => { if (_cmbNationality != null) _cmbNationality.Visible = lblNationality.Visible; };
        }
    }
}
