using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using Nop.Core.Domain.Security;

namespace Nop.Plugin.Editor.CKEditor
{
    /// <summary>
    /// Represents System helper
    /// </summary>
    public class SystemHelper
    {

        #region Constants

        /// <summary>
        /// Represents payment processor transaction mode
        /// </summary>
        public enum AccessMode
        {
            /// <summary>
            /// Testing
            /// </summary>
            NoAccess = 0,

            /// <summary>
            /// Testing
            /// </summary>
            Testing = 1,

            /// <summary>
            /// Configure 
            /// </summary>
            Configure = 2,
        }

        #endregion

        #region Methods

        PermissionRecord _noAccess = new PermissionRecord
        {
            Name = "Admin area. No Access",
            SystemName = "NoAccess",
            Category = "Access"
        };

        PermissionRecord _managePlugins = new PermissionRecord
        {
            Name = "Admin area.Manage Plugins",
            SystemName = "Configuration.ManagePlugins",
            Category = "Configuration"
        };

        public PermissionRecord GetAccessPermission(AccessMode accessMode)
        {
            switch (accessMode)
            {
                case AccessMode.Testing:
                    return _managePlugins;

                case AccessMode.Configure:
                    return _managePlugins;

            }

            return _noAccess;

        }

        public Guid DateToGuid(DateTime accessDate)
        {
            var date = new DateTime(accessDate.Year, accessDate.Month, accessDate.Day, accessDate.Hour, accessDate.Minute, 00);
            var guid = date.ToGuid();
            return guid;
        }

        public DateTime GuidToDate(Guid accessGuid)
        {
            var date = accessGuid.ToDateTime();
            return date;
        }

        public string GetDomainNameFromHost(string url)
        {
            string domain = string.Empty;
            if (url.Contains("localhost"))
                domain = "localhost";
            if (url != null)
            {
                string[] names = url.Split(".");
                for (int count = 1; count < names.Count(); count++)
                {
                    domain += names[count];
                    if (count + 1 < names.Count())
                        domain += ".";
                }
            }

            domain = domain.Replace("/", "");
            return domain;
        }

        #endregion

        public class CKEditorDefaults
        {

            /// <summary>
            /// Mollie payment method system name
            /// </summary>
            public static string SystemName => "Editor.CKEditor";

        }
    }

    public static class StringExtension
    {
        public static string Truncate(this string s, int length)
        {
            return string.IsNullOrEmpty(s) || s.Length <= length ? s
                : length <= 0 ? string.Empty
                : s.Substring(0, length);
        }
    }

    public static class DateTimeExtensions
    {
        public static Guid ToGuid(this DateTime dt)
        {
            var bytes = BitConverter.GetBytes(dt.Ticks);

            Array.Resize(ref bytes, 16);

            return new Guid(bytes);
        }
    }

    public static class GuidExtensions
    {
        public static DateTime ToDateTime(this Guid guid)
        {
            var bytes = guid.ToByteArray();

            Array.Resize(ref bytes, 8);

            return new DateTime(BitConverter.ToInt64(bytes));
        }
    }
}

