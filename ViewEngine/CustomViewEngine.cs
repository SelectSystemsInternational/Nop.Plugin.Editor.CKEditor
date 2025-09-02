using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Services.Plugins;
using Nop.Core.Infrastructure;

using Nop.Plugin.Editor.CKEditor.Settings;

namespace Nop.Plugin.Editor.CKEditor.ViewEngine
{
    /// <summary>
    /// Google Analytics plugin
    public class CustomViewEngine : IViewLocationExpander
    {

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            //nothing to do here.
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // Only need to load the view locations for Admin functions when the plugin is installed and enabled

            var pluginService = EngineContext.Current.Resolve<IPluginService>();
            var plugin = pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Editor.CKEditor", LoadPluginsMode.All);
            var enabled = EngineContext.Current.Resolve<CKEditorSettings>().Enable;
            if (enabled && plugin != null)
            {

                if (context.AreaName == "Admin")
                {    
                    viewLocations = new string[] {
                        $"/Plugins/SSI.Editor.CKEditor/Views/Admin/{{0}}.cshtml",
                        $"/Plugins/SSI.Editor.CKEditor/Views/Admin/{{1}}/{{0}}.cshtml",
                        $"/Plugins/SSI.Editor.CKEditor/Views/Admin/{{2}}/{{1}}/{{0}}.cshtml",
                        $"/Plugins/SSI.Editor.CKEditor/Views/Shared/{{1}}/{{0}}.cshtml",
                        $"/Plugins/SSI.Editor.CKEditor/Views/Shared/{{0}}.cshtml",
                        $"/Plugins/SSI.Editor.CKEditor/Views/Shared/EditorTemplates/{{0}}.cshtml",
                        $"/Plugins/SSI.Editor.CKEditor/Views/{{2}}/{{1}}/{{0}}.cshtml"

                    }.Concat(viewLocations);
                }
                else if (context.ControllerName == "Customer" && context.ViewName == "EditorTemplates/RichEditor")
                {
                    viewLocations = new string[] {
                        $"/Plugins/SSI.Editor.CKEditor/Views/FrontView/Shared/EditorTemplates/RichEditor.cshtml",
                    }.Concat(viewLocations);
                }
                else if (context.ViewName == "Alert")
                {
                    viewLocations = new string[] {
                        $"/Areas/Admin/Views/Shared/Alert.cshtml",
                    }.Concat(viewLocations);
                }
            }
            
            return viewLocations;
        }
    }
}