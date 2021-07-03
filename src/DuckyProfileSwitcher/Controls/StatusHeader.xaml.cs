using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DuckyProfileSwitcher.Controls
{
    /// <summary>
    /// Interaction logic for StatusHeader.xaml
    /// </summary>
    public partial class StatusHeader : UserControl
    {
        private const string StatusHeaderEnabledBrush = nameof(StatusHeaderEnabledBrush);
        private const string StatusHeaderDisabledBrush = nameof(StatusHeaderDisabledBrush);

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(bool), typeof(StatusHeader), new PropertyMetadata(PropertyChanged));
        public static readonly DependencyProperty TextTrueProperty = DependencyProperty.Register(nameof(TextTrue), typeof(string), typeof(StatusHeader), new PropertyMetadata(PropertyChanged));
        public static readonly DependencyProperty TextFalseProperty = DependencyProperty.Register(nameof(TextFalse), typeof(string), typeof(StatusHeader), new PropertyMetadata(PropertyChanged));

        public StatusHeader()
        {
            InitializeComponent();
        }

        public bool Value
        {
            get => (bool)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
                Update();
            }
        }

        public string TextTrue
        {
            get => (string)GetValue(TextTrueProperty);
            set
            {
                SetValue(TextTrueProperty, value);
                Update();
            }
        }

        public string TextFalse
        {
            get => (string)GetValue(TextFalseProperty);
            set
            {
                SetValue(TextFalseProperty, value);
                Update();
            }
        }

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatusHeader sh)
            {
                sh.Update();
            }
        }

        private void Update()
        {
            if (Value)
            {
                label.Text = TextTrue;
                dot.Fill = (Brush)FindResource(StatusHeaderEnabledBrush);
            }
            else
            {
                label.Text = TextFalse;
                dot.Fill = (Brush)FindResource(StatusHeaderDisabledBrush);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
            Update();
        }
    }
}