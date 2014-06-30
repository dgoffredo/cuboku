using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Sudokudos
{
    class Themes
    {
        public const int nSheets = 3;

        public class CellColors
        {
            public SolidColorBrush[] y = new SolidColorBrush[nSheets];

            public CellColors(params SolidColorBrush[] brushes)
            {
                Debug.Assert(brushes.Length == y.Length);
                for (int i = 0; i < y.Length; ++i)
                    y[i] = brushes[i];
            }
        }

        public class ThemeSetting
        {
            public ImageBrush bgBrush;

            public class argb
            {
                public byte alpha { get; set; }
                public byte red { get; set; }
                public byte green { get; set; }
                public byte blue { get; set; }
            }

            public argb[] colors = new argb[nSheets];

            public ThemeSetting(ImageBrush brush, params byte[] colorValues)
            {
                Debug.Assert(colorValues.Length == 4 * colors.Length);

                bgBrush = brush;
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = new argb { alpha = colorValues[4 * i],      red = colorValues[4 * i + 1], 
                                           green = colorValues[4 * i + 2], blue = colorValues[4 * i + 3] };
                }
            }
        }

        public class Theme
        {
            public CellColors colors;
            public ImageBrush background;

            public Theme(ThemeSetting setting)
            {
                background = setting.bgBrush;

                var clrs = setting.colors;
                colors = new CellColors(
                    new SolidColorBrush(
                        Color.FromArgb(clrs[0].alpha, clrs[0].red, clrs[0].green, clrs[0].blue)),
                    new SolidColorBrush(
                        Color.FromArgb(clrs[1].alpha, clrs[1].red, clrs[1].green, clrs[1].blue)),
                    new SolidColorBrush(
                        Color.FromArgb(clrs[2].alpha, clrs[2].red, clrs[2].green, clrs[2].blue)));
            }
        }
        
        public enum PresetTheme : int { Eye, Dark, Red, Blue, Starry };

        ThemeSetting[] presets;

        ImageBrush[] backgrounds;

        ImageBrush newImageLike(ImageBrush orig)
        {
            ImageBrush ret = new ImageBrush();
            ret.ImageSource = orig.ImageSource;
            return ret;
        }

        public Themes(ImageBrush[] bgBrushes)
        {
            backgrounds = bgBrushes;
            
            presets = new ThemeSetting[] {
                new ThemeSetting(newImageLike(backgrounds[(int)PresetTheme.Eye]), new byte[] { 
                    0xEE, 0xFF, 0x00, 0x88,
                    0xEE, 0x88, 0x00, 0x00,
                    0xEE, 0x00, 0x44, 0x00 }),

                new ThemeSetting(newImageLike(backgrounds[(int)PresetTheme.Dark]), new byte[] { 
                    0xEE, 0x11, 0xA5, 0xF5,
                    0xEE, 0xA5, 0x00, 0xF5,
                    0xEE, 0x11, 0xBB, 0x99 }),

                new ThemeSetting(newImageLike(backgrounds[(int)PresetTheme.Red]), new byte[] { 
                    0xEE, 0x88, 0x00, 0x40,
                    0xEE, 0x00, 0x40, 0x88,
                    0xEE, 0x74, 0xA8, 0x20 }),

                new ThemeSetting(newImageLike(backgrounds[(int)PresetTheme.Blue]), new byte[] { 
                    0xEE, 0x11, 0xA5, 0xF5,
                    0xEE, 0xA5, 0x00, 0xF5,
                    0xEE, 0x11, 0x88, 0x99 }),

                new ThemeSetting(newImageLike(backgrounds[(int)PresetTheme.Starry]), new byte[] { 
                    0xEE, 0x88, 0x00, 0x40,
                    0xEE, 0x00, 0x40, 0x88,
                    0xEE, 0x74, 0xA8, 0x20 }),
            };
        }

        public Theme getPreset(PresetTheme preset)
        {
            return new Theme(presets[(int)preset]);
        }
    }
}
