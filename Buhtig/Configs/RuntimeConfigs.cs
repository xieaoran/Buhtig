using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buhtig.Configs
{
    public static class RuntimeConfigs
    {
        public static LocalWorkSpaceConfig LocalWorkSpaceConfig { get; set; }

        public static void LoadDefaults()
        {
            LocalWorkSpaceConfig = new LocalWorkSpaceConfig();
        }
    }
}
