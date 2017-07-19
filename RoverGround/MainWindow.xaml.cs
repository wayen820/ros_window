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

using Ros_CSharp;
using XmlRpc_Wrapper;
using Messages;
using System.Threading;

namespace RoverGround
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            label.Content = string.Format("键盘控制关闭");
        }
        void doConnect()
        {
            //开启键盘控制节点
            ROS.Init(new string[0], "tele_key");
            nh = new NodeHandle();

            pub = nh.advertise<Messages.geometry_msgs.Twist>("/cmd_vel", 1, false);
        }
        public static MainWindow Instance = null;
        Publisher<Messages.geometry_msgs.Twist> pub;
        NodeHandle nh;
        bool isTele_key = false;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (!isTele_key)
            {
                speed = 1.0;
                label.Content = string.Format("油门：{0}", speed);

                isTele_key = true;
            }
            else
            {
                label.Content = string.Format("键盘控制关闭");
                isTele_key = false;
            }
        }
        double speed = 1.0;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isTele_key) return;
            double linear = 0.0, angular = 0.0;
            switch(e.Key)
            {
                case Key.W:
                    linear = 0.15;
                    break;
                case Key.S:
                    linear = -0.15;
                    break;
                case Key.A:
                    angular = 0.9;
                    break;
                case Key.D:
                    angular = -0.9;
                    break;
                case Key.O:
                    speed += 0.1;
                    break;
                case Key.P:
                    speed -= 0.1;
                    break;
            }
            pubCmdMsg(linear, angular, speed);
            label.Content = string.Format("油门：{0}",speed);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
                if (!isTele_key) return;
                pubCmdMsg(0, 0, speed);
        }
        public void pubCmdMsg(double linear,double angular,double speed)
        {
            Messages.geometry_msgs.Twist msg;
            msg = new Messages.geometry_msgs.Twist();
            msg.linear = new Messages.geometry_msgs.Vector3();
            msg.linear.x=linear * speed;

            msg.angular = new Messages.geometry_msgs.Vector3();
            msg.angular.z = angular * speed;
            pub.publish(msg);
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            doConnect();
        }
        RoverGround.TouchControl.TouchControl touchControl;
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if(touchControl==null)
            {
                touchControl = new TouchControl.TouchControl();
                touchControl.ShowForm();
            }
            else
            {
                touchControl.CloseForm();
                touchControl = null;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            ROS.shutdown();
            ROS.waitForShutdown();
            base.OnClosed(e);
        }
    }
}
