using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Ros_CSharp;
using d = System.Drawing;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;
namespace DrawTools.ROS
{
    public class KeyControl
    {
        public KeyControl(DrawArea owner)
        {
            this.owner = owner;
            nh = new NodeHandle();

            pub = nh.advertise<Messages.geometry_msgs.Twist>("/cmd_vel", 1, false);

        }
        public const float STOP_DISTANCE = 0.20f;
        
        Publisher<Messages.geometry_msgs.Twist> pub;
        NodeHandle nh;
        public bool isTele_key = false;
        double speed = 1.0;
        public void StartControl()
        {
            SubscribeToLaserScan("/laser_pointcloud_base_link");
            isTele_key = true;
        }
        public void ExitControl()
        {
            if (laserSub != null)
            {
                laserSub.shutdown();
                laserSub = null;
            }
            isTele_key = false;
        }
        public void Process_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isTele_key) return;
            double linear = 0.0, angular = 0.0;
            switch (e.KeyCode)
            {
                case Keys.W:
                    linear = 0.15;
                    break;
                case Keys.S:
                    linear = -0.15;
                    break;
                case Keys.A:
                    angular = 0.9;
                    break;
                case Keys.D:
                    angular = -0.9;
                    break;
                case Keys.O:
                    speed += 0.1;
                    break;
                case Keys.P:
                    speed -= 0.1;
                    break;
            }
            pubCmdMsg(linear, angular, speed);
            //label.Content = string.Format("油门：{0}", speed);
        }

        public void Process_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isTele_key) return;
            pubCmdMsg(0, 0, speed);
        }
        public void pubCmdMsg(double linear, double angular, double speed)
        {
            Messages.geometry_msgs.Twist msg;
            msg = new Messages.geometry_msgs.Twist();
            msg.linear = new Messages.geometry_msgs.Vector3();
            msg.linear.x = linear * speed>0?(block_up?0:linear*speed):(block_down?0:linear*speed);

            msg.angular = new Messages.geometry_msgs.Vector3();
            msg.angular.z = angular * speed;
            pub.publish(msg);
        }
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
            laserSub = nh.subscribe<sm.PointCloud>(topic, 1,  i =>owner.BeginInvoke(new Action(()=>
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
                }
                catch (Exception ex)
                {

                }

            })));
        }
    }
}
