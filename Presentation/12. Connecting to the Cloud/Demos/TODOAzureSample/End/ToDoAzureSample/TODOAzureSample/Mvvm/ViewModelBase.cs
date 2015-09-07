using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TODOAzureSample.Common;
using TODOAzureSample.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace TODOAzureSample.Mvvm
{
    public abstract class ViewModelBase : BindableBase, Services.NavigationService.INavigable
    {
        public virtual void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state) { /* nothing by default */ }
        public virtual Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending) { return Task.FromResult<object>(null); }
        public virtual void OnNavigatingFrom(Services.NavigationService.NavigatingEventArgs args) { /* nothing by default */ }

        public NavigationService NavigationService { get; set; }
        public Common.StateItems SessionState { get { return BootStrapper.Current.SessionState; } }

    }
}