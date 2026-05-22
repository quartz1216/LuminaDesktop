using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;

namespace LuminaDesktop.Controls
{
    public partial class PositionIcon : UserControl
    {
        public static readonly DependencyProperty ActivePositionProperty = 
            DependencyProperty.Register("ActivePosition", typeof(string), typeof(PositionIcon), new PropertyMetadata(string.Empty, OnActivePositionChanged));

        public string ActivePosition
        {
            get { return (string)GetValue(ActivePositionProperty); }
            set { SetValue(ActivePositionProperty, value); }
        }

        public PositionIcon()
        {
            InitializeComponent();
        }

        private static void OnActivePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PositionIcon icon)
            {
                icon.UpdateHighlight((string)e.NewValue);
            }
        }

        private void UpdateHighlight(string pos)
        {
            var inactive = new SolidColorBrush(Color.FromRgb(105, 105, 105)); // #696969
            var active = Brushes.White;

            dotTopLeft.Fill = pos == "TopLeft" ? active : inactive;
            dotTop.Fill = pos == "Top" ? active : inactive;
            dotTopRight.Fill = pos == "TopRight" ? active : inactive;
            dotLeft.Fill = pos == "Left" ? active : inactive;
            dotRight.Fill = pos == "Right" ? active : inactive;
            dotBottomLeft.Fill = pos == "BottomLeft" ? active : inactive;
            dotBottom.Fill = pos == "Bottom" ? active : inactive;
            dotBottomRight.Fill = pos == "BottomRight" ? active : inactive;
        }
    }
}
