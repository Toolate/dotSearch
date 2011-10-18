using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using dotSearch.Bot;
using System.Diagnostics;

namespace dotSearch.BotApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void RunBot_button_Click(object sender, RoutedEventArgs e)
        {
            BotPage botPage = new BotPage(StartUrl_textBox.Text, System.Int32.Parse(Depth_textBox.Text));
            Stopwatch sw = new Stopwatch();
            sw.Start();
             botPage.Run();
            sw.Stop();
            TimeElapsedValue_label.Content = sw.ElapsedMilliseconds.ToString();
        }

        private void Depth_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Depth_textBox.Text = Depth_slider.Value.ToString();
        }

        private void Depth_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Depth_slider.Value = Double.Parse(Depth_textBox.Text);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dotSearch.BotApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
