using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace TODOAzureSample.Mvvm
{
    public abstract class ViewModelBase : BindableBase, Services.NavigationService.INavigatable
    {
        public virtual Task OnNavigatedToAsync(string parameter, NavigationMode mode)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnNavigatedFromAsync(bool suspending)
        {
            return Task.FromResult<object>(null);
        }

    }
}