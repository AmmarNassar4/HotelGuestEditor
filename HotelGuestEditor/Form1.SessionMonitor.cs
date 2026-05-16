using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotelGuestEditor
{
    public partial class Form1
    {
        private readonly Timer _sessionMonitorTimer = new Timer { Interval = 10000 };
        private bool _sessionMonitorBusy;
        private bool _sessionMonitorAutoRestartEnabled;
        private bool _sessionMonitorRestartPending;
        private bool _sessionMonitorManualStopRequested;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartSessionMonitor();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopSessionMonitor();
            base.OnFormClosed(e);
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
    }
}
