using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.Serialization;

using tf.net;

using Ros_CSharp;
using d = System.Drawing;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;

namespace DrawTools.ROS
{
    public class DrawLaserScan : DrawObject
    {
        private List<Point> pointList;

        /// <summary>
        ///  Graphic objects for hit test
        /// </summary>
        private GraphicsPath areaPath = null;

        private Pen areaPen = null;
        private Region areaRegion = null;

        public DrawLaserScan(DrawArea owner)
        {
            this.owner = owner;
            pointList = new List<Point>();
            pointList.Add(new Point(10, 52));
            pointList.Add(new Point(10, 25));
            pointList.Add(new Point(10, 187));
            ZOrder = 0;

            //DrawPen = DrawingPens.SetCurrentPen(DrawingPens.PenType.RedPen);
            //PenType = DrawingPens.PenType.Generic;

            Color = Color.Red;
            PenWidth = 1;

            Initialize();
        }
        public override void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Brush b = new SolidBrush(Color.Red);
            Pen pen;
            if (DrawPen == null)
                pen = new Pen(Color, PenWidth);
            else
                pen = (Pen)DrawPen.Clone();
            GraphicsPath gp = new GraphicsPath();
            foreach (Point pt in pointList)
            {
               
                gp.AddEllipse(new Rectangle(pt,new Size(1,1)));
            }
            g.DrawPath(pen, gp);
            g.FillPath(b, gp);
            gp.Dispose();
            b.Dispose();
            pen.Dispose();
        }
        public override DrawObject Clone()
        {
            return null;
        }
        public override bool IntersectsWith(Rectangle rectangle)
        {
            CreateObjects();

            return AreaRegion.IsVisible(rectangle);
        }
        protected void Invalidate()
        {
            if (AreaPath != null)
            {
                AreaPath.Dispose();
                AreaPath = null;
            }

            if (AreaPen != null)
            {
                AreaPen.Dispose();
                AreaPen = null;
            }

            if (AreaRegion != null)
            {
                AreaRegion.Dispose();
                AreaRegion = null;
            }
        }

        /// <summary>
        /// Create graphic objects used for hit test.
        /// </summary>
        protected virtual void CreateObjects()
        {
            if (AreaPath != null)
                return;

            // Create path which contains wide line
            // for easy mouse selection
            AreaPath = new GraphicsPath();
            foreach (Point pt in pointList)
            {
                AreaPath.AddLine(pt, pt);
            }
            // Take into account the width of the pen used to draw the actual object
            //AreaPen = new Pen(Color.Black, PenWidth < 7 ? 7 : PenWidth);
            //// Prevent Out of Memory crash when startPoint == endPoint
            //if (startPoint.Equals((Point)endPoint))
            //{
            //    endPoint.X++;
            //    endPoint.Y++;
            //}
            //AreaPath.AddLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
            //AreaPath.Widen(AreaPen);
            // Rotate the path about it's center if necessary
            //if (Rotation != 0)
            //{
            //    RectangleF pathBounds = AreaPath.GetBounds();
            //    Matrix m = new Matrix();
            //    m.RotateAt(Rotation, new PointF(pathBounds.Left + (pathBounds.Width / 2), pathBounds.Top + (pathBounds.Height / 2)), MatrixOrder.Append);
            //    AreaPath.Transform(m);
            //    m.Dispose();
            //}

            // Create region from the path
            AreaRegion = new Region(AreaPath);
        }

        protected GraphicsPath AreaPath
        {
            get { return areaPath; }
            set { areaPath = value; }
        }

        protected Pen AreaPen
        {
            get { return areaPen; }
            set { areaPen = value; }
        }
        protected Region AreaRegion
        {
            get { return areaRegion; }
            set { areaRegion = value; }
        }
        DrawArea owner;
        Transformer tfer;
        NodeHandle laserhandle;
        private Subscriber<sm.PointCloud> laserSub;
        public void SubscribeToLaserScan(string topic)
        {
            if (laserhandle == null)
                laserhandle = new NodeHandle();
            while(!laserhandle.ok)
            {
                System.Threading.Thread.Sleep(10);
            }
            if (laserSub != null && laserSub.topic != topic)
            {
                laserSub.shutdown();
                laserSub = null;
            }
            if (laserSub != null)
                return;
            if (tfer == null)
                tfer = new Transformer(false);

            Console.WriteLine("Subscribing to laser_pointCloud at:= " + topic);
            laserSub = laserhandle.subscribe<sm.PointCloud>(topic, 1, i => owner.BeginInvoke(new Action(() =>
            {
                try
                {
                    pointList.Clear();
                    for (int j=0;j<i.points.Length;j++)
                    {
                        Point worldPoint = owner.Map2World(new PointF((float)i.points[j].x, (float)i.points[j].y));
                        pointList.Add(worldPoint);
                    }

                    //if (!tfer.waitForTransform("map", "base_laser", i.header.stamp, new m.Duration(new Messages.TimeData(1, 0)), null))
                    //{
                    //    Console.WriteLine("drawLaserScan:wait for tf timeout");
                    //    return;
                    //}
                    //emTransform ret = new emTransform();
                    //if (tfer.lookupTransform("map", i.header.frame_id, i.header.stamp, out ret))
                    //{
                    //    pointList.Clear();
                    //    int index = 0;
                    //    double angle = i.angle_min;
                    //    if (i.angle_min >= i.angle_max) return;
                    //    do
                    //    {
                    //        angle += i.angle_increment;
                    //        if (i.ranges[index] >= i.range_min && i.ranges[index] <= i.range_max)
                    //        {
                    //            emVector3 vector = new emVector3(i.ranges[index] * Math.Cos(angle), i.ranges[index] * Math.Sin(angle), 0.0);


                    //            emVector3 origin = new emVector3(0, 0, 0);
                    //            emVector3 output = (ret * vector) - (ret * origin);
                    //            //emVector3 output = ret * vector;

                    //            Point worldPoint = owner.Map2World(new PointF((float)output.x, (float)output.y));
                    //            pointList.Add(worldPoint);
                    //            //pointList.Add(new Point((int)vector.x*,(int)vector.y*10));
                    //        }
                    //        index++;
                    //    } while (angle < i.range_max && index < i.ranges.Length);
                    //}
                    Invalidate();
                    //owner.Invalidate();
                    owner.LastDataUpdateTime = DateTime.Now;
                }
                catch (Exception ex)
                {

                }

            })));
        }
    //}
    }
}
