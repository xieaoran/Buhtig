using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buhtig.Entities.User;

namespace Buhtig.Entities.Base
{
    public interface IPostProcessRequired
    {
        void PostProcess(Team team);
    }
}
