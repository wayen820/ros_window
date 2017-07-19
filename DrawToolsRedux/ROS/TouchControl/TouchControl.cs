using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfApplication1;
using System.Threading;
using System.Reflection;
using RoverGround;

using Ros_CSharp;
using d = System.Drawing;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;
using DrawTools;

namespace RoverGround.TouchControl
{
    public class TouchControl
    {
        public TouchControl(DrawArea owner)
        {
            this.owner = owner;
            nh = new NodeHandle();

            pub = nh.advertise<Messages.geometry_msgs.Twist>("/cmd_vel", 1, false);

            engineCtlForm = new WpfApplication1.MainWindow(this);
            directCtrlForm = new WpfApplication1.Window1(this);
        }
        static readonly object syncObj = new object();

        public double Speed = 1.0;
        public double Linear = 0;
        public double Angular = 0;
        public const float STOP_DISTANCE = 0.20f;

        WpfApplication1.MainWindow engineCtlForm;
        WpfApplication1.Window1 directCtrlForm;
        public void PubMsg()
        {
            pubCmdMsg(Linear, Angular, Speed);
        }
        public void ShowForm()
        {
            if (engineCtlForm.IsVisible || directCtrlForm.IsVisible ) return;

            System.Windows.Forms.Application.EnableVisualStyles();

            engineCtlForm.Topmost = true;
            engineCtlForm.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            engineCtlForm.Left = System.Windows.SystemParameters.PrimaryScreenWidth - engineCtlForm.Width-60;
            engineCtlForm.Top= (System.Windows.SystemParameters.PrimaryScreenHeight - engineCtlForm.Height) / 2;
            engineCtlForm.Show();

            directCtrlForm.Topmost = true;
            directCtrlForm.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            directCtrlForm.Left = 20;
            directCtrlForm.Top = (System.Windows.SystemParameters.PrimaryScreenHeight - directCtrlForm.Height) / 2;
            directCtrlForm.Show();

            SubscribeToLaserScan("/laser_pointcloud_base_link");
        }
        public void CloseForm()
        {
            engineCtlForm.Close();
            directCtrlForm.Close();

            if (laserSub != null)
            {
                laserSub.shutdown();
                laserSub = null;
            }
        }


        private void pubCmdMsg(double linear, double angular, double speed)
        {
            Messages.geometry_msgs.Twist msg;
            msg = new Messages.geometry_msgs.Twist();
            msg.linear = new Messages.geometry_msgs.Vector3();
            msg.linear.x = linear * speed > 0 ? (block_up ? 0 : linear * speed) : (block_down ? 0 : linear * speed);

            msg.angular = new Messages.geometry_msgs.Vector3();
            msg.angular.z = angular * speed;
            pub.publish(msg);
        }
        Publisher<Messages.geometry_msgs.Twist> pub;
        NodeHandle nh;

        DrawArea owner;
        private Subscriber<sm.PointCloud> laserSub;
        bool block_up = false, block_down = false;
        public void SubscribeToLaserScan(string topic)
        {
            if (laserSub != null && laserSub.topic != topic)
            {
                laserSub.shutdown();
                laserSub = null;
            }
            if (laserSub != null)
                return;

            Console.WriteLine("Subscribing to laser_pointCloud at:= " + topic);
            laserSub = nh.subscribe<sm.PointCloud>(topic, 1, i =>owner.BeginInvoke(new Action(()=>
            {
                try
                {
                    //block_down = false;
                    //block_down = false;
                    bool isfree = true;
                    for (int j = 0; j < i.points.Length; j++)
                    {
                        if (Math.Abs(i.points[j].y) < STOP_DISTANCE && Math.Abs(i.points[j].x) < STOP_DISTANCE)
                        {
                            if (i.points[j].x > 0)
                            {
                                block_up = true;
                            }
                            else
                                block_down = true;
                            isfree = false;
                        }

                    }
                    if (isfree)
                    {
                        block_up = false;
                        block_down = false;
                    }
                    PubMsg();
                }
                catch (Exception ex)
                {

                }

            })));
        }
    }

}
