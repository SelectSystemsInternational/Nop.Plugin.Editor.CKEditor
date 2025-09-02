using Nop.Core.Configuration;

namespace Nop.Plugin.Editor.CKEditor.Settings
{
    public class CKEditorSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the Enable flag
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the Security Public Key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the Security Private Key
        /// </summary>
        public string PrivateKey { get; set; }

    }
}