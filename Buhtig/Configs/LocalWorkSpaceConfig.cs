using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Buhtig.Resources.Strings;

namespace Buhtig.Configs
{
    public class LocalWorkSpaceConfig
    {
        public string WorkingDir { get; set; }
        public string RepoPathFormat { get; set; }

        public LocalWorkSpaceConfig()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appName = Language.AppName;
            WorkingDir = Path.Combine(appData, appName, StaticConfigs.RepoFolder);
            RepoPathFormat = Path.Combine(WorkingDir, "{0}");
        }
    }
}