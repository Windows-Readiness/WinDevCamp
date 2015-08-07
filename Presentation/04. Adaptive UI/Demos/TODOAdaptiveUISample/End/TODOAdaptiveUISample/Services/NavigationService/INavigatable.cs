using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace TODOAdaptiveUISample.Services.NavigationService
{
    public interface INavigatable
    {
        Task OnNavigatedToAsync(string parameter, NavigationMode mode);
        Task OnNavigatedFromAsync(bool suspending);
    }
}
