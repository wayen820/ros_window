using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DrawTools;
using System.Drawing;

using tf.net;
using Ros_CSharp;
using d = System.Drawing;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;

namespace DrawTools.ROS
{
    internal class ToolGoal: ToolObject
    {
        public ToolGoal()
        {
            nh = new NodeHandle();
            goalPub = nh.advertise<gm.PoseStamped>("/move_base_simple/goal", 1);
        }
        int goalDrawID=-1;
        NodeHandle nh;
        Publisher<gm.PoseStamped> goalPub;
        /// <summary>
        /// Left nous button is pressed
        /// </summary>
        /// <param name="drawArea"></param>
        /// <param name="e"></param>
        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            //delete last goal draw object
            if(goalDrawID != -1)
            {
                int al = drawArea.TheLayers.ActiveLayerIndex;

                drawArea.TheLayers[al].Graphics.DeleteObjectByID(goalDrawID);
            }

            Point p = drawArea.BackTrackMouse(new Point(e.X, e.Y));

            if (drawArea.CurrentPen == null)
            {
                DrawEllipse  drawGoal = new DrawEllipse(p.X, p.Y, 4, 4, drawArea.LineColor, drawArea.FillColor, drawArea.DrawFilled, drawArea.LineWidth);
                AddNewObject(drawArea, drawGoal);
                goalDrawID = drawGoal.ID;
            }
            else
            {
                DrawEllipse drawGoal = new DrawEllipse(p.X, p.Y, 4, 4, drawArea.PenType, drawArea.FillColor, drawArea.DrawFilled);
                AddNewObject(drawArea, drawGoal);
                goalDrawID = drawGoal.ID;
            }

            PointF mapPoint = drawArea.World2Map(p);

            gm.PoseStamped pose = new Messages.geometry_msgs.PoseStamped();
            pose.header = new m.Header();
            pose.header.frame_id = "map";
            pose.header.stamp = Ros_CSharp.ROS.GetTime();
            pose.pose = new gm.Pose();
            pose.pose.position = new gm.Point();
            pose.pose.position.x = mapPoint.X;
            pose.pose.position.y = mapPoint.Y;

            //emQuaternion quaternion= tf.net.emQuaternion.FromRPY(new emVector3(0,90, 0));
            //pose.pose.orientation = quaternion.ToMsg();
            pose.pose.orientation = new gm.Quaternion();
            pose.pose.orientation.x = 0;
            pose.pose.orientation.y = 0;
            pose.pose.orientation.z = 0;
            pose.pose.orientation.w = 1;
            goalPub.publish(pose);
        }


        /// <summary>
        /// Mouse is moved, left mouse button is pressed or none button is pressed
        /// </summary>
        /// <param name="drawArea"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
        }


        /// <summary>
        /// Left mouse button is released
        /// </summary>
        /// <param name="drawArea"></param>
        /// <param name="e"></param>
        public override void OnMouseUp(DrawArea drawArea, MouseEventArgs e)
        {
        }
    }
}
