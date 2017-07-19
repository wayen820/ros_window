using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Ros_CSharp;
using d = System.Drawing;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;

namespace DrawTools.ROS
{
    public class PassiveControl
    {
        public PassiveControl(DrawArea owner)
        {
            this.owner = owner;
            nh = new NodeHandle();

            pub = nh.advertise<Messages.geometry_msgs.Twist>("/cmd_vel", 1, false);
            timer = new Timer((obj) =>
              {
                  double linear = 0.0, angular = 0.0;
                  if (block_up && !block_down)
                  {
                      linear = -0.15;
                  }
                  else if(!block_up && block_down)
                  {
                      linear = 0.15;
                  }
                  else if(block_up && block_down)
                  {
                      angular = 0.9;
                  }
                  pubCmdMsg(linear, angular, speed);
              },null, 1000, 500);
        }
        Timer timer;
        DrawArea owner;
        public const float STOP_DISTANCE = 0.20f;
        double speed = 1.0;
        Publisher<Messages.geometry_msgs.Twist> pub;
        NodeHandle nh;

        public void StartControl()
        {
            SubscribeToLaserScan("/laser_pointcloud_base_link");
        }
        public void ExitControl()
        {
            if (laserSub != null)
            {
                laserSub.shutdown();
                laserSub = null;
            }
        }
        public void pubCmdMsg(double linear, double angular, double speed)
        {
            Messages.geometry_msgs.Twist msg;
            msg = new Messages.geometry_msgs.Twist();
            msg.linear = new Messages.geometry_msgs.Vector3();
            msg.linear.x = linear * speed > 0 ? (block_up ? 0 : linear * speed) : (block_down ? 0 : linear * speed);

            msg.angular = new Messages.geometry_msgs.Vector3();
            msg.angular.z = angular * speed;
            pub.publish(msg);
        }
        private Subscriber<sm.PointCloud> laserSub;
        bool block_up = false, block_down = false;

        private void SubscribeToLaserScan(string topic)
        {
            if (laserSub != null && laserSub.topic != topic)
            {
                laserSub.shutdown();
                laserSub = null;
            }
            if (laserSub != null)
                return;

            Console.WriteLine("Subscribing to laser_pointCloud at:= " + topic);
            laserSub = nh.subscribe<sm.PointCloud>(topic, 1, i =>
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

            });
        }
    }

}
