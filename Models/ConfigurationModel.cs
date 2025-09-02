using System;
using System.Collections.Generic;
using System.Text;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Editor.CKEditor.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Editor.CKEditor.Fields.Enable")]
        public bool Enable { get; set; }

        public bool Enable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Editor.CKEditor.Fields.PublicKey")]
        public string PublicKey { get; set; }

        [NopResourceDisplayName("Plugins.Editor.CKEditor.Fields.PrivateKey")]
        public string PrivateKey { get; set; }

    }
}
