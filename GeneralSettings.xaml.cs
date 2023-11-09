using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoftAnime
{
    /// <summary>
    /// Interaction logic for GeneralSettings.xaml
    /// </summary>
    public partial class GeneralSettings : UserControl
    {
        public GeneralSettings()
        {
            InitializeComponent();
        }

        private void ThemeColorHex_GotFocus(object sender, RoutedEventArgs e)
        {
            ThemeColorHex.SelectAll();
        }

        private void ThemeColorHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex reg = new Regex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
            Match match = reg.Match(ThemeColorHex.Text);
            BrushConverter cnv = new BrushConverter();
            TextBox textBox = (TextBox)sender;

            if(textBox.Text.Length > 7)
            {
                textBox.Text = textBox.Text.Substring(0, 7);
                textBox.CaretIndex = textBox.Text.Length;

            } else if (match.Success)
            {
                System.Windows.Media.Brush brush = (System.Windows.Media.Brush)cnv.ConvertFromString(ThemeColorHex.Text);
                if (ColorPickerView != null)
                {
                    ColorPickerView.Background = brush;
                }
            }

        }
    }
}
