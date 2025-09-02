using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;

using Nop.Plugin.Editor.CKEditor.Settings;
using Nop.Services.Security;


namespace Nop.Plugin.Editor.CKEditor
{
    /// <summary>
    /// Google Analytics plugin
    /// </summary>
    public class CKEditorPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly IWebHelper _webHelper;
        protected readonly ISettingService _settingService;
        protected readonly IStoreContext _storeContext;
        protected readonly ILogger _logger;
        protected readonly IEncryptionService _encryptionService;

        SystemHelper _systemHelper = new SystemHelper();

        #endregion

        #region Ctor

        public CKEditorPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IStoreContext storeContext,
            ILogger logger,
            IEncryptionService encryptionService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _storeContext = storeContext;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CKEditor/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            string dateInstall = DateTime.Now.Date.ToString();
            var demoDateInstall = DateTime.Parse(dateInstall).AddDays(14);
            Guid pKeyInstall = _systemHelper.DateToGuid(demoDateInstall);
            string publicKeyInstall = pKeyInstall.ToString();
            string urlInstall = _systemHelper.GetDomainNameFromHost(store.Url);
            string privateKeyInstall = _encryptionService.EncryptText(urlInstall, publicKeyInstall);

            string message = string.Format("Installing plugin {0} for Store {1} with PublicKey {2} and PrivateKey {3}",
                SystemHelper.CKEditorDefaults.SystemName, urlInstall, publicKeyInstall, privateKeyInstall);

            await _logger.InformationAsync(message);

            var settings = new CKEditorSettings
            {
                PublicKey = publicKeyInstall,
                PrivateKey = privateKeyInstall,
            };
            
            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Editor.CKEditor.Note.Restart"] = "Please restart the application once the configuration has been modified.",
                ["Plugins.Editor.CKEditor.Fields.Enable"] = "Enable",
                ["Plugins.Editor.CKEditor.Fields.Enable.Hint"] = "Please click to enable the plugin",
                ["Plugins.Editor.CKEditor.Fields.PublicKey"] = "Licence Key",
                ["Plugins.Editor.CKEditor.Fields.PublicKey.Hint"] = "Enter the Licence Key supplied after plugin purchase",
                ["Plugins.Editor.CKEditor.Fields.PrivateKey"] = "Private Key",
                ["Plugins.Editor.CKEditor.Fields.PrivateKey.Hint"] = "Enter the Private Licence Key supplied via email after plugin purchase",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<CKEditorSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Editor.CKEditor");

            await base.UninstallAsync();
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;
    }
}