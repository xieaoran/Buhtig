using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buhtig.Resources.Strings;
using Telerik.Windows.Controls;

namespace Buhtig.Helpers
{
    public static class DialogHelper
    {
        public static DialogParameters BuildDialogParameters(string content)
        {
            var dialogParameters = new DialogParameters
            {
                Content = content,
                Header = Language.AppName,
                OkButtonContent = Language.OK,
                CancelButtonContent = Language.Cancel
            };
            return dialogParameters;
        }
        public static DialogParameters BuildDialogParametersWithCallback(string content, EventHandler<WindowClosedEventArgs> callback)
        {
            var dialogParameters = new DialogParameters
            {
                Content = content,
                Header = Language.AppName,
                OkButtonContent = Language.OK,
                CancelButtonContent = Language.Cancel,
                Closed = callback
            };
            return dialogParameters;
        }

        public static void PromptException(Exception exception)
        {
            RadWindow.Alert(BuildDialogParameters(exception.Message));
        }
    }
}
