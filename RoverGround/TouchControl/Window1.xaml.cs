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
using System.Windows.Shapes;
using RoverGround.TouchControl;

namespace WpfApplication1
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1(TouchControl controller)
        {
            InitializeComponent();
            this.controller = controller;
        }
        TouchControl controller;
        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_TouchUp(object sender, TouchEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button_TouchDown(object sender, TouchEventArgs e)
        {
            controller.Linear = -0.15;
            controller.PubMsg();
        }

        private void button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            controller.Linear = -0.15;
            controller.PubMsg();
        }

        private void button_TouchLeave(object sender, TouchEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button3_TouchLeave(object sender, TouchEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button3_TouchDown(object sender, TouchEventArgs e)
        {
            controller.Linear = 0.15;
            controller.PubMsg();
        }

        private void button3_TouchUp(object sender, TouchEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button3_MouseLeave(object sender, MouseEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            controller.Linear = 0.15;
            controller.PubMsg();
        }

        private void button3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            controller.Linear = 0;
            controller.PubMsg();
        }

        private void button2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            controller.Angular = 0.9;
            controller.PubMsg();
        }

        private void button2_MouseLeave(object sender, MouseEventArgs e)
        {
            controller.Angular = 0;
            controller.PubMsg();
        }

        private void button2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            controller.Angular = 0;
            controller.PubMsg();
        }

        private void button2_TouchUp(object sender, TouchEventArgs e)
        {
            controller.Angular = 0;
            controller.PubMsg();
        }

        private void button2_TouchLeave(object sender, TouchEventArgs e)
        {
            controller.Angular = 0;
            controller.PubMsg();
        }

        private void button2_TouchDown(object sender, TouchEventArgs e)
        {
            controller.Angular = 0.9;
            controller.PubMsg();
        }

        private void button1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            controller.Angular = -0.9;
            controller.PubMsg();
        }

        private void button1_MouseLeave(object sender, MouseEventArgs e)
        {
            controller.Angular = 0;
            controller.PubMsg();
        }

        private void button1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            controller.Angular = 0;
            controller.PubMsg();
        }

        private void button1_TouchUp(object sender, TouchEventArgs e)
        {
            controller.Angular = 0;
            controller.PubMsg();
        }

        private void button1_TouchLeave(object sender, TouchEventArgs e)
        {
            controller.Angular = 0 ;
            controller.PubMsg();
        }

        private void button1_TouchDown(object sender, TouchEventArgs e)
        {
            controller.Angular = -0.9;
            controller.PubMsg();
        }

        private void button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            controller.Linear = -0.15;
            controller.PubMsg();
        }

        private void button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

    }
}
