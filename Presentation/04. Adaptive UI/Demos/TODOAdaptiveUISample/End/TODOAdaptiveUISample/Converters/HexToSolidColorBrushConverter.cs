using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TODOAdaptiveUISample.Converters
{
    class HexToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var hexCode = value as string;
                var color = new Color();
                color.A = byte.Parse(hexCode.Substring(1, 2), NumberStyles.AllowHexSpecifier);
                color.R = byte.Parse(hexCode.Substring(3, 2), NumberStyles.AllowHexSpecifier);
                color.G = byte.Parse(hexCode.Substring(5, 2), NumberStyles.AllowHexSpecifier);
                color.B = byte.Parse(hexCode.Substring(7, 2), NumberStyles.AllowHexSpecifier);
                return new SolidColorBrush(color);
            }
            catch { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
