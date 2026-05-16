using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using READERDEMO;

namespace HotelGuestEditor
{
    public sealed class DocumentInfo
    {
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string NameEnglish { get; set; }
        public string NameArabic { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string GenderCode { get; set; }
        public string GenderArabic { get; set; }
        public string NationalityCode { get; set; }
        public string Nationality { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
    }

    public sealed class RegulaPassportScanService : IDisposable
    {
        private readonly RegulaReader _reader = new RegulaReader();

        public RegulaPassportScanService()
        {
            _reader.OnRFIDRequest += OnRfidRequestInternal;
        }

        public async Task<DocumentInfo> ScanDocumentAsync(CancellationToken ct, bool standbyMode = false)
        {
            await Task.Run(() =>
            {
                if (!_reader.Connected)
                {
                    _reader.Connect();
                    _reader.InBackground = standbyMode;
                }
            }, ct);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void finishedHandler()
            {
                tcs.TrySetResult(true);
            }

            _reader.OnProcessingFinished += finishedHandler;
            try
            {
                await Task.Run(() => _reader.GetImages(), ct);

                using (ct.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task;
                }
            }
            finally
            {
                _reader.OnProcessingFinished -= finishedHandler;
            }

            object xmlObj = _reader.CheckReaderResultXML((int)eRPRM_ResultType.RPRM_ResultType_OCRLexicalAnalyze, 0, 0);
            string xml = xmlObj != null ? Convert.ToString(xmlObj) : string.Empty;

            if (string.IsNullOrWhiteSpace(xml))
                throw new InvalidOperationException("No OCR XML result returned from Regula.");

            return Parse(xml);
        }

        private static DocumentInfo Parse(string ocrXml)
        {
            var xdoc = XDocument.Parse(ocrXml);
            var fieldInfos = xdoc.Descendants("Document_Field_Analysis_Info").ToList();

            XElement FindByFieldType(int fieldType)
            {
                return fieldInfos.FirstOrDefault(x => (int?)x.Element("FieldType") == fieldType);
            }

            XElement FindByTypeLcid(int type, int lcid)
            {
                return fieldInfos.FirstOrDefault(x => (int?)x.Element("Type") == type && (int?)x.Element("LCID") == lcid);
            }

            string GetValue(XElement node)
            {
                if (node == null) return null;
                string visual = ((string)node.Element("Field_Visual"))?.Trim();
                string mrz = ((string)node.Element("Field_MRZ"))?.Trim();
                return !string.IsNullOrWhiteSpace(visual) ? visual : mrz;
            }

            string GetFieldByFieldType(int fieldType) => GetValue(FindByFieldType(fieldType));
            string GetFieldByTypeAndLcid(int type, int lcid) => GetValue(FindByTypeLcid(type, lcid));

            string docTypeCode = GetFieldByFieldType(0);
            string nationalityCode = GetFieldByFieldType(26) ?? GetFieldByFieldType(1);
            string nationality = GetFieldByFieldType(38) ?? GetFieldByFieldType(11);
            nationalityCode = NormalizeNationalityFromPassport(nationalityCode, nationality);
            string genderCode = GetFieldByFieldType(12);
            string genderArabic = GetFieldByTypeAndLcid(12, 1025);

            return new DocumentInfo
            {
                DocumentType = MapDocumentType(docTypeCode),
                DocumentNumber = GetFieldByFieldType(2),
                NameEnglish = GetFieldByFieldType(25) ?? $"{GetFieldByFieldType(8)} {GetFieldByFieldType(9)}".Trim(),
                NameArabic = GetFieldByTypeAndLcid(25, 1025),
                DateOfBirth = GetFieldByFieldType(5),
                Gender = MapGender(genderCode, genderArabic),
                GenderCode = genderCode,
                GenderArabic = genderArabic,
                NationalityCode = nationalityCode,
                Nationality = nationality,
                IssueDate = GetFieldByFieldType(4),
                ExpiryDate = GetFieldByFieldType(3)
            };
        }

        private static string NormalizeNationalityFromPassport(string code, string name)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                string normalized = code.Trim().ToUpperInvariant();
                if (normalized.Length >= 3)
                    return normalized.Substring(0, 3);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (CountryCatalog.TryFindCode(CountryCatalog.Load(), name, out string mappedCode))
                    return mappedCode;

                string normalizedName = name.Trim().ToUpperInvariant();
                if (normalizedName.Length == 3)
                    return normalizedName;
            }

            return string.Empty;
        }

        private static string MapDocumentType(string docTypeCode)
        {
            if (string.IsNullOrWhiteSpace(docTypeCode)) return null;
            switch (docTypeCode)
            {
                case "P": return "Passport";
                case "I": return "ID Card";
                default: return docTypeCode;
            }
        }

        private static string MapGender(string genderCode, string genderArabic)
        {
            if (!string.IsNullOrWhiteSpace(genderCode))
            {
                switch (genderCode.Trim().ToUpperInvariant())
                {
                    case "M": return "Male";
                    case "F": return "Female";
                    default: return "Other";
                }
            }

            if (!string.IsNullOrWhiteSpace(genderArabic))
            {
                if (genderArabic.Contains("ذكر")) return "Male";
                if (genderArabic.Contains("أنث")) return "Female";
                return "Other";
            }

            return null;
        }

        private void OnRfidRequestInternal(object requestXml)
        {
            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(requestXml != null ? requestXml.ToString() : "<SDK_Request/>");
                _reader.RFID_ResponseXML = "<SDK_Response/>";
            }
            catch
            {
                _reader.RFID_ResponseXML = "<SDK_Response/>";
            }
        }
        public void CancelCurrentScan()
        {
            try
            {
                _reader.InBackground = false;
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            try { _reader.OnRFIDRequest -= OnRfidRequestInternal; } catch { }
            try { _reader.Disconnect(); } catch { }
        }
    }
}
