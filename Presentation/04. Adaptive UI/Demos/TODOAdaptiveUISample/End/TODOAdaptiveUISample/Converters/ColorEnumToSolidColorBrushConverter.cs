using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TODOAdaptiveUISample.Models;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TODOAdaptiveUISample.Converters
{
    class ColorEnumToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var color = (TaskColor)value;

                switch(color)
                {
                    case TaskColor.Purple:
                        return new SolidColorBrush(Color.FromArgb(255, 145, 60, 205));
                    case TaskColor.Pink:
                        return new SolidColorBrush(Color.FromArgb(255, 241, 95, 116));
                    case TaskColor.Orange:
                        return new SolidColorBrush(Color.FromArgb(255, 247, 109, 60));
                    case TaskColor.Yellow:
                        return new SolidColorBrush(Color.FromArgb(255, 247, 216, 66));
                    case TaskColor.Teal:
                        return new SolidColorBrush(Color.FromArgb(255, 44, 168, 194));
                    case TaskColor.Green:
                        return new SolidColorBrush(Color.FromArgb(255, 152, 203, 74));
                    case TaskColor.Gray:
                        return new SolidColorBrush(Color.FromArgb(255, 131, 144, 152));
                    case TaskColor.Blue:
                        return new SolidColorBrush(Color.FromArgb(255, 84, 129, 230));
                    default:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
            catch { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
