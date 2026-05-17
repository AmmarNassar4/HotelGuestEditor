from pathlib import Path

project_dir = Path(__file__).resolve().parents[1] / "HotelGuestEditor"
form_path = project_dir / "Form1.cs"
settings_path = project_dir / "AppSettings.cs"
appsettings_path = project_dir / "appsettings.json"

text = form_path.read_text(encoding="utf-8-sig")

old_constants = '''        // ─── GuestGate / SignalR ──────────────────────────────────────────────
        private const string DefaultBaseUrl = "https://guestgate-ramada.ibaapps.work";
        private const string FixedKid = "K1";
        private const string FixedTemplateId = "T_Checkin";
'''
new_settings = '''        // ─── GuestGate / SignalR ──────────────────────────────────────────────
        private readonly AppSettings _settings = AppSettings.Load();
        private string DefaultBaseUrl => _settings.BaseUrl;
        private string FixedKid => _settings.KioskId;
        private string FixedTemplateId => _settings.TemplateId;
        private string KioskDisplayName => _settings.KioskName;
'''
if old_constants in text:
    text = text.replace(old_constants, new_settings)

old_ctor = '''            InitializeGenderCombo();

            _fixedControls = BuildControlMap();
'''
new_ctor = '''            InitializeGenderCombo();
            SetMsg("Ready");

            _fixedControls = BuildControlMap();
'''
if old_ctor in text:
    text = text.replace(old_ctor, new_ctor, 1)

old_setmsg = '''        private void SetMsg(string text)
        {
            Text = string.IsNullOrWhiteSpace(text)
                ? "Reservation Guest Editor"
                : $"Reservation Guest Editor — {text}";
        }
'''
new_setmsg = '''        private void SetMsg(string text)
        {
            string kioskName = string.IsNullOrWhiteSpace(KioskDisplayName)
                ? FixedKid
                : KioskDisplayName;

            string prefix = $"Reservation Guest Editor — Kiosk: {kioskName}";
            Text = string.IsNullOrWhiteSpace(text)
                ? prefix
                : $"{prefix} — {text}";
        }
'''
if old_setmsg in text:
    text = text.replace(old_setmsg, new_setmsg, 1)
else:
    print("WARNING: SetMsg block was not found. No status/title update was applied.")

form_path.write_text(text, encoding="utf-8")

settings = settings_path.read_text(encoding="utf-8-sig")
if 'public string KioskName' not in settings:
    settings = settings.replace(
        '        public string KioskId { get; private set; } = "K1";\n',
        '        public string KioskId { get; private set; } = "K1";\n        public string KioskName { get; private set; } = "K1";\n'
    )
    settings = settings.replace(
        '            settings.KioskId = ReadString(guestGate, "KioskId", settings.KioskId);\n',
        '            settings.KioskId = ReadString(guestGate, "KioskId", settings.KioskId);\n            settings.KioskName = ReadString(guestGate, "KioskName", settings.KioskId);\n'
    )
settings_path.write_text(settings, encoding="utf-8")

appsettings = appsettings_path.read_text(encoding="utf-8-sig")
if '"KioskName"' not in appsettings:
    appsettings = appsettings.replace(
        '    "KioskId": "K3",\n',
        '    "KioskId": "K3",\n    "KioskName": "Kiosk K3",\n'
    )
appsettings_path.write_text(appsettings, encoding="utf-8")

print("Applied appsettings and kiosk title/status fix.")
