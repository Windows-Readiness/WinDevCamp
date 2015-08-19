using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TODOAdaptiveUISample.Views
{
    public sealed partial class InkNotesControl : UserControl
    {
        const int minPenSize = 2;
        const int penSizeIncrement = 2;
        int penSize;

        public InkNotesControl()
        {
            this.InitializeComponent();

            penSize = minPenSize + penSizeIncrement * PenThickness.SelectedIndex;

            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Red;
            drawingAttributes.Size = new Size(penSize, penSize);
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;

            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
        }

        #region SaveLoad
        public async Task Load(Uri uri)
        {
            if (uri != null)
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                if (file != null)
                {
                    using (var stream = await file.OpenSequentialReadAsync())
                    {
                        try
                        {
                            await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(stream);
                        }
                        catch (Exception ex)
                        {
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }
                        }
                    }
                    
                }
            }
        }

        private async Task<Uri> SaveToFile(StorageFile file)
        {
            Uri uri = null;

            if (inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                try
                {
                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);
                        uri = new Uri("ms-appdata:///local/" + file.Name);
                    }
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }


            return uri;
        }

        public async Task<Uri> Save()
        {
            var filename = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".gif";
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename);
            return await SaveToFile(file);
        }

        public async Task<Uri> Save(Uri uri)
        {
            if (uri != null)
            {
                return await SaveToFile(await StorageFile.GetFileFromApplicationUriAsync(uri));
            }
            else
            {
                throw new NullReferenceException("Uri canot be null");
            }
        }

        #endregion

        #region InkPresenterPropertiesUpdate
        void OnPenColorChanged(object sender, RoutedEventArgs e)
        {
            if (inkCanvas != null)
            {
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();

                // Use button's background to set new pen's color
                var btnSender = sender as Button;
                var brush = btnSender.Background as Windows.UI.Xaml.Media.SolidColorBrush;

                drawingAttributes.Color = brush.Color;
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        void OnPenThicknessChanged(object sender, RoutedEventArgs e)
        {
            if (inkCanvas != null)
            {
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
                penSize = minPenSize + penSizeIncrement * PenThickness.SelectedIndex;
                string value = ((ComboBoxItem)PenType.SelectedItem).Content.ToString();
                if (value == "Highlighter" || value == "Calligraphy")
                {
                    // Make the pen tip rectangular for highlighter and calligraphy pen
                    drawingAttributes.Size = new Size(penSize, penSize * 2);
                }
                else
                {
                    drawingAttributes.Size = new Size(penSize, penSize);
                }
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        void OnPenTypeChanged(object sender, RoutedEventArgs e)
        {
            if (inkCanvas != null)
            {
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
                string value = ((ComboBoxItem)PenType.SelectedItem).Content.ToString();

                if (value == "Ballpoint")
                {
                    drawingAttributes.Size = new Size(penSize, penSize);
                    drawingAttributes.PenTip = PenTipShape.Circle;
                    drawingAttributes.DrawAsHighlighter = false;
                    drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.Identity;
                }
                else if (value == "Highlighter")
                {
                    // Make the pen rectangular for highlighter
                    drawingAttributes.Size = new Size(penSize, penSize * 2);
                    drawingAttributes.PenTip = PenTipShape.Rectangle;
                    drawingAttributes.DrawAsHighlighter = true;
                    drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.Identity;
                }
                if (value == "Calligraphy")
                {
                    drawingAttributes.Size = new Size(penSize, penSize * 2);
                    drawingAttributes.PenTip = PenTipShape.Rectangle;
                    drawingAttributes.DrawAsHighlighter = false;

                    // Set a 45 degree rotation on the pen tip
                    double radians = 45.0 * Math.PI / 180;
                    drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.CreateRotation((float)radians);
                }
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        void OnClear(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
        }

        private void TouchInkingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
        }

        private void ErasingModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
        }

        private void ErasingModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
        }
        #endregion

        #region Recognizer
        private async void Reco_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                var recognizer = new InkRecognizerContainer();
                var recognitionResults = await recognizer.RecognizeAsync(inkCanvas.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

                string result = "";
                //justACanvas.Children.Clear();

                if (recognitionResults.Count > 0)
                {
                    foreach (var r in recognitionResults)
                    {
                        var candidates = r.GetTextCandidates();
                        result += " " + candidates[0];

                        //var rect = r.BoundingRect;
                        //var text = new TextBlock();
                        //text.Text = candidates[0];
                        //justACanvas.Children.Add(text);
                        //Canvas.SetTop(text, rect.Bottom + 5);
                        //Canvas.SetLeft(text, rect.Left);
                    }
                }
                else
                    result = "NO TEXT RECOGNIZED!";

                recoResult.Text = result;
            }
        }

        #endregion

        #region Selection

        #endregion

        #region click
        private void OpenOptions(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = true;
        }

        private void CloseOptions(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = false;
        }
        #endregion
    }
}
