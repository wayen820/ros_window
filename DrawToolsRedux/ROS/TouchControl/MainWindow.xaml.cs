using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RoverGround.TouchControl;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(TouchControl controller)
        {
            InitializeComponent();
            this.touchControl = controller;
            slider.Value = touchControl.Speed;
        }

        RoverGround.TouchControl.TouchControl touchControl;
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.DragMove();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(touchControl!=null)
            touchControl.Speed = slider.Value;
        }

        private void slider_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void slider_TouchLeave(object sender, TouchEventArgs e)
        {

        }

        private void slider_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {

        }
    }
}
