using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TODOAdaptiveUISample.Converters
{
    class DateTimeToHumanReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var dt = (DateTime)value;
                var ts = DateTime.Now - dt;

                if (ts.Days < 1)
                    return -ts.Days + " days left";
                else if (ts.Days == -1)
                    return "1 day left";
                else if (ts.Days == 0)
                    return "due today";
                else if (ts.Days == 1)
                    return "overdue by 1 day";
                else
                    return "overdue by " + ts.Days + " days";
            }
            catch { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
