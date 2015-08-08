using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AdaptiveListDemo
{
    public sealed partial class ItemTemplateControl : UserControl
    {
        public ItemTemplateControl()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += Current_SizeChanged;
            this.OnApplyTemplate();
        }

        protected override void OnApplyTemplate()
        {
            SetUXProperties(Window.Current.Bounds.Width);
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            SetUXProperties(e.Size.Width);
        }

        void SetUXProperties(double windowWidth)
        {
            mainPanel.Width = windowWidth;
        }
    }
}
