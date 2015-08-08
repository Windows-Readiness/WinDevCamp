using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveListDemo.ViewModel
{
    public class MainViewModel
    {
        private ObservableCollection<Data> _items;
        public ObservableCollection<Data> Items
        {
            get
            {
                return _items;
            }

            set
            {
                _items = value;
            }
        }
        public MainViewModel()
        {
            Items = new ObservableCollection<Data>();
            Items.Clear();
            Items.Add(new Data { BgColor= new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black), Number="1", Name = "Fireside", Artist = "Brett Bixby", Duration = "3:12", ImageUri = new Uri("ms-appx:///Assets/emptyHeart.png", UriKind.Absolute) });
            Items.Add(new Data { BgColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.DarkSlateGray), Number = "2", Name = "The Longer I Run", Artist = "Peter Bradley Adams", Duration = "3:12", ImageUri = new Uri("ms-appx:///Assets/emptyHeart.png", UriKind.Absolute) });
            Items.Add(new Data { BgColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black), Number = "3", Name = "I Don't Feel It Anymore (Song of the Sparrow)", Artist = "William Fitzsimmons", Duration = "3:12", ImageUri = new Uri("ms-appx:///Assets/fullHeart.png", UriKind.Absolute) });
            Items.Add(new Data { BgColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.DarkSlateGray), Number = "4", Name = "Hurricane", Artist = "Mindy Smith", Duration = "3:12", ImageUri = new Uri("ms-appx:///Assets/emptyHeart.png", UriKind.Absolute) });
            Items.Add(new Data { BgColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black), Number = "5", Name = "Shine", Artist = "Benjamin Francis Leftwich", Duration = "3:12", ImageUri = new Uri("ms-appx:///Assets/emptyHeart.png", UriKind.Absolute) });
        }
    }
}
