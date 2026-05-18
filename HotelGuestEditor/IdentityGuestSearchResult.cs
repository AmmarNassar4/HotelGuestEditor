using System;
using System.Data;

namespace HotelGuestEditor
{
    public sealed class IdentityGuestSearchResult
    {
        public string GuestTitle { get; private set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string MiddleName { get; private set; } = string.Empty;
        public string GuestCode { get; private set; } = string.Empty;
        public string GuestAddress { get; private set; } = string.Empty;
        public string GuestCountry { get; private set; } = string.Empty;
        public string GuestCity { get; private set; } = string.Empty;
        public string GuestState { get; private set; } = string.Empty;
        public string GuestZip { get; private set; } = string.Empty;
        public string GuestTelephone { get; private set; } = string.Empty;
        public string GuestMobile { get; private set; } = string.Empty;
        public string CompanyName { get; private set; } = string.Empty;
        public string Nationality { get; private set; } = string.Empty;
        public string Passport { get; private set; } = string.Empty;
        public string Gender { get; private set; } = string.Empty;
        public DateTime? DateOfBirth { get; private set; }
        public DateTime? Anniversary { get; private set; }
        public string Smoking { get; private set; } = string.Empty;
        public string WorkPermit { get; private set; } = string.Empty;
        public string GuestEmail { get; private set; } = string.Empty;
        public string BlackList { get; private set; } = string.Empty;
        public DateTime? PassportIssueDate { get; private set; }

        public static IdentityGuestSearchResult FromDataRow(DataRow row)
        {
            return new IdentityGuestSearchResult
            {
                GuestTitle = ReadString(row, "Guest Title"),
                FirstName = ReadString(row, "First Name"),
                LastName = ReadString(row, "Last Name"),
                MiddleName = ReadString(row, "Middle Name"),
                GuestCode = ReadString(row, "Guest Code"),
                GuestAddress = ReadString(row, "Guest Address"),
                GuestCountry = ReadString(row, "Guest Country"),
                GuestCity = ReadString(row, "Guest City"),
                GuestState = ReadString(row, "Guest State"),
                GuestZip = ReadString(row, "Guest Zip"),
                GuestTelephone = ReadString(row, "Guest Telephone"),
                GuestMobile = ReadString(row, "Guest Mobile"),
                CompanyName = ReadString(row, "Company Name"),
                Nationality = ReadString(row, "Nationality"),
                Passport = ReadString(row, "Passport"),
                Gender = ReadString(row, "Gender"),
                DateOfBirth = ReadDate(row, "Date Of Birth"),
                Anniversary = ReadDate(row, "Anniversary"),
                Smoking = ReadString(row, "Smoking"),
                WorkPermit = ReadString(row, "Work Permit"),
                GuestEmail = ReadString(row, "Guest Email"),
                BlackList = ReadString(row, "Black List"),
                PassportIssueDate = ReadDate(row, "Passport issue date")
            };
        }

        private static string ReadString(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return string.Empty;

            return row[columnName]?.ToString()?.Trim() ?? string.Empty;
        }

        private static DateTime? ReadDate(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return null;

            if (row[columnName] is DateTime date)
                return date;

            if (DateTime.TryParse(row[columnName]?.ToString(), out DateTime parsed))
                return parsed;

            return null;
        }
    }
}

