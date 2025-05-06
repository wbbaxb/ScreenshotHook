using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace ScreenshotHook.Presentation.ObservableObjects
{
    public class WatermarkObservableObject : ObservableObject
    {
        private string _text;
        private int _fontSize;
        private string _fontFamily;
        private byte _colorR;
        private byte _colorG;
        private byte _colorB;
        private byte _colorA;

        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged(); }
        }

        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged(); }
        }

        public string FontFamily
        {
            get { return _fontFamily; }
            set { _fontFamily = value; OnPropertyChanged(); }
        }

        public byte ColorR
        {
            get { return _colorR; }
            set { _colorR = value; OnPropertyChanged(); OnPropertyChanged(nameof(Color)); }
        }

        public byte ColorG
        {
            get { return _colorG; }
            set { _colorG = value; OnPropertyChanged(); OnPropertyChanged(nameof(Color)); }
        }

        public byte ColorB
        {
            get { return _colorB; }
            set { _colorB = value; OnPropertyChanged(); OnPropertyChanged(nameof(Color)); }
        }

        public byte ColorA
        {
            get { return _colorA; }
            set { _colorA = value; OnPropertyChanged(); OnPropertyChanged(nameof(Color)); }
        }

        public Color Color => Color.FromArgb(ColorA, ColorR, ColorG, ColorB);
    }
}