using ResourceManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ResourceManager.Helpers
{
    public static class StyleHelper
    {
        public static void UpdateNavButtonStyle(
            RadioButton button,
            bool isEnabled,
            string? disabledToolTip = null)
        {

            button.SetCurrentValue(FrameworkElement.StyleProperty, (Style)button.FindResource(
                isEnabled
                    ? "ModernMenuButtonStyle"
                    : "DisabledMenuButtonStyle"
            ));

            button.SetCurrentValue(FrameworkElement.ToolTipProperty, isEnabled
            ? null : disabledToolTip);
        }
    }
}
