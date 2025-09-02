using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

using Nop.Plugin.Editor.CKEditor.Models;
using Nop.Plugin.Editor.CKEditor.Settings;

namespace Nop.Plugin.Editor.CKEditor.Controllers
{

    public class CKEditorController : BasePluginController
    {

        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly INotificationService _notificationService;
        protected readonly ISettingService _settingService;
        protected readonly IStoreContext _storeContext;
        protected readonly CKEditorSettings _cKEditorSettings;
        protected readonly ILogger _logger;
        protected readonly IPermissionService _permissionService;
        protected readonly IEncryptionService _encryptionService;

        SystemHelper _systemHelper = new SystemHelper();

        #endregion

        #region Ctor

        public CKEditorController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            CKEditorSettings cKEditorSettings,
            ILogger logger,
            IPermissionService permissionService,
            IEncryptionService encryptionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _cKEditorSettings = cKEditorSettings;
            _logger = logger;
            _permissionService = permissionService;
            _encryptionService = encryptionService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ckEditorSettings = await _settingService.LoadSettingAsync<CKEditorSettings>(storeScope);

            bool access = await AuthorizeAsync(SystemHelper.AccessMode.Configure, ckEditorSettings.PublicKey, ckEditorSettings.PrivateKey);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                Enable = ckEditorSettings.Enable,
                PublicKey = ckEditorSettings.PublicKey,
                PrivateKey = ckEditorSettings.PrivateKey,
            };

            if (model.Enable)
            {
                if (!access)
                    model.Enable = false;
            }

            if (storeScope > 0)
            {                
                model.Enable_OverrideForStore = await _settingService.SettingExistsAsync(ckEditorSettings, x => x.Enable, storeScope);
            }

            if (!access)
                _notificationService.ErrorNotification("Demo Version has Expired - Please enter the Licence Key");

            return View("~/Plugins/SSI.Editor.CKEditor/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        [AutoValidateAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ckEditorSettings = await _settingService.LoadSettingAsync<CKEditorSettings>(storeScope);

            ckEditorSettings.Enable = model.Enable;
            ckEditorSettings.PublicKey = model.PublicKey;

            if (model.Enable)
            {
                bool access = await AuthorizeAsync(SystemHelper.AccessMode.Configure, model.PublicKey, model.PrivateKey);
                if (!access)
                    model.Enable = false;
            }

            await _settingService.SaveSettingAsync(ckEditorSettings, x => x.PublicKey, 0, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ckEditorSettings, x => x.Enable, model.Enable_OverrideForStore, storeScope, true);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }


        public async Task<bool> AuthorizeAsync(SystemHelper.AccessMode requestedAccess, string publicKey = "", string privateKey = "")
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            string dateInstall = DateTime.Now.Date.ToString();
            var demoDateInstall = DateTime.Parse(dateInstall).AddDays(14);
            Guid pKeyInstall = _systemHelper.DateToGuid(demoDateInstall);
            string publicKeyInstall = pKeyInstall.ToString();
            string urlInstall = _systemHelper.GetDomainNameFromHost(store.Url);
            string privateKeyInstall = _encryptionService.EncryptText(urlInstall, publicKeyInstall);

            string pKey = string.Empty;
            bool hasKey = false;
            string key = publicKey != "" ? publicKey : _cKEditorSettings.PublicKey;
            if (key != null)
            {
                if (key.ToString() == "c9d59e3a-ee08-427b-99a1-7967d291d66b") //System Key
                    hasKey = true;
                else if (key.ToString().Contains("4750b6dd-517b-4b03-a6e3-618053cab589"))
                {
                    // Purchase Key
                    string sKey = _encryptionService.EncryptText("Licenced", key);
                    pKey = (privateKey != "" ? privateKey : _cKEditorSettings.PrivateKey);
                    if (sKey != pKey)
                    {
                        _cKEditorSettings.PrivateKey = sKey;
                        await _settingService.SaveSettingAsync(_cKEditorSettings);
                        hasKey = true;
                    }
                    else if (sKey == pKey)
                    {
                        hasKey = true;
                    }
                }
                else if (key.ToString() != "4750b6dd-517b-4b03-a6e3-618053cab589")
                {
                    try
                    {
                        pKey = _encryptionService.EncryptText(urlInstall, key);
                        if (pKey == (privateKey != "" ? privateKey : _cKEditorSettings.PrivateKey))
                        {
                            Guid guid = Guid.Parse(key);
                            DateTime keyDate = _systemHelper.GuidToDate(guid);
                            if (keyDate > DateTime.Now)
                                hasKey = true;
                        }
                    }
                    catch (Exception exc)
                    {
                        string message = string.Format("Error checking licence for Store {1} with PublicKey {2} and PrivateKey {3}",
                            SystemHelper.CKEditorDefaults.SystemName, urlInstall, key, pKey);
                        await _logger.ErrorAsync(message, exc);
                    }
                }
            }

            if (hasKey)
            {
                PermissionRecord acesss = _systemHelper.GetAccessPermission(requestedAccess);
                return await _permissionService.AuthorizeAsync(acesss);
            }
            else
            {
                string message = string.Format("Plugin {0} was installed for Store {1} with PublicKey {2} and PrivateKey {3}",
                    SystemHelper.CKEditorDefaults.SystemName, urlInstall, key, pKey);
                await _logger.InformationAsync(message);
            }

            return false;
        }

        #endregion
    }
}