using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Collections;

using tf.net;

using Ros_CSharp;
using d = System.Drawing;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;

namespace DrawTools.ROS
{
    public class DrawPath: DrawLine
    {
        // Last Segment start and end points
        private Point startPoint;
    private Point endPoint;

    private ArrayList pointArray; // list of points
    private Cursor handleCursor;

    private const string entryLength = "Length";
    private const string entryPoint = "Point";

    public Point StartPoint
    {
        get { return startPoint; }
        set { startPoint = value; }
    }

    public Point EndPoint
    {
        get { return endPoint; }
        set { endPoint = value; }
    }

    /// <summary>
    /// Clone this instance
    /// </summary>
    public override DrawObject Clone()
    {
        //DrawPolyLine drawPolyLine = new DrawPolyLine();

        //drawPolyLine.startPoint = startPoint;
        //drawPolyLine.endPoint = endPoint;
        //drawPolyLine.pointArray = pointArray;

        //FillDrawObjectFields(drawPolyLine);
        //return drawPolyLine;
        return null;
    }
    DrawArea owner;
    public DrawPath(DrawArea owner)
    {
        pointArray = new ArrayList();

        LoadCursor();
        Initialize();

        Color = Color.YellowGreen;
        PenWidth = 1;

        this.owner = owner;
    }

    public DrawPath(int x1, int y1, int x2, int y2, DrawArea owner)
    {
        pointArray = new ArrayList();
        pointArray.Add(new Point(x1, y1));
        pointArray.Add(new Point(x2, y2));

        LoadCursor();
        Initialize();

        this.owner = owner;
    }

    public DrawPath(int x1, int y1, int x2, int y2, DrawingPens.PenType p, DrawArea owner)
    {
        pointArray = new ArrayList();
        pointArray.Add(new Point(x1, y1));
        pointArray.Add(new Point(x2, y2));
        DrawPen = DrawingPens.SetCurrentPen(p);
        PenType = p;

        LoadCursor();
        Initialize();

        this.owner = owner;
    }

    public DrawPath(int x1, int y1, int x2, int y2, Color lineColor, int lineWidth, DrawArea owner)
    {
        pointArray = new ArrayList();
        pointArray.Add(new Point(x1, y1));
        pointArray.Add(new Point(x2, y2));
        Color = lineColor;
        PenWidth = lineWidth;

        LoadCursor();
        Initialize();

        this.owner = owner;
    }

    public override void Draw(Graphics g)
    {

        if (pointArray.Count == 0) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        Pen pen;

        if (DrawPen == null)
            pen = new Pen(Color, PenWidth);
        else
            pen = DrawPen.Clone() as Pen;

        Point[] pts = new Point[pointArray.Count];
        for (int i = 0; i < pointArray.Count; i++)
        {
            Point px = (Point)pointArray[i];
            pts[i] = px;
        }
        byte[] types = new byte[pointArray.Count];
        for (int i = 0; i < pointArray.Count; i++)
            types[i] = (byte)PathPointType.Line;

        GraphicsPath gp = new GraphicsPath(pts, types);
        // Rotate the path about it's center if necessary
        if (Rotation != 0)
        {
            RectangleF pathBounds = gp.GetBounds();
            Matrix m = new Matrix();
            m.RotateAt(Rotation, new PointF(pathBounds.Left + (pathBounds.Width / 2), pathBounds.Top + (pathBounds.Height / 2)), MatrixOrder.Append);
            gp.Transform(m);
        }
        g.DrawPath(pen, gp);
        //g.DrawCurve(pen, pts);
        gp.Dispose();
        if (pen != null)
            pen.Dispose();
    }

    public void AddPoint(Point point)
    {
        pointArray.Add(point);
    }

    public override int HandleCount
    {
        get { return pointArray.Count; }
    }

    /// <summary>
    /// Get handle point by 1-based number
    /// </summary>
    /// <param name="handleNumber"></param>
    /// <returns></returns>
    public override Point GetHandle(int handleNumber)
    {
        if (handleNumber < 1)
            handleNumber = 1;
        if (handleNumber > pointArray.Count)
            handleNumber = pointArray.Count;
        return ((Point)pointArray[handleNumber - 1]);
    }

    public override Cursor GetHandleCursor(int handleNumber)
    {
        return handleCursor;
    }

    public override void MoveHandleTo(Point point, int handleNumber)
    {
        if (handleNumber < 1)
            handleNumber = 1;

        if (handleNumber > pointArray.Count)
            handleNumber = pointArray.Count;
        pointArray[handleNumber - 1] = point;
        Dirty = true;
        Invalidate();
    }

    public override void Move(int deltaX, int deltaY)
    {
        int n = pointArray.Count;

        for (int i = 0; i < n; i++)
        {
            Point point;
            point = new Point(((Point)pointArray[i]).X + deltaX, ((Point)pointArray[i]).Y + deltaY);
            pointArray[i] = point;
        }
        Dirty = true;
        Invalidate();
    }

    public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
    {
        //info.AddValue(
        //    String.Format(CultureInfo.InvariantCulture,
        //                  "{0}{1}-{2}",
        //                  entryLength, orderNumber, objectIndex),
        //    pointArray.Count);

        //int i = 0;
        //foreach (Point p in pointArray)
        //{
        //    info.AddValue(
        //        String.Format(CultureInfo.InvariantCulture,
        //                      "{0}{1}-{2}-{3}",
        //                      new object[] { entryPoint, orderNumber, objectIndex, i++ }),
        //        p);
        //}
        //base.SaveToStream(info, orderNumber, objectIndex);
    }

    public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
    {
        //int n = info.GetInt32(
        //    String.Format(CultureInfo.InvariantCulture,
        //                  "{0}{1}-{2}",
        //                  entryLength, orderNumber, objectIndex));

        //for (int i = 0; i < n; i++)
        //{
        //    Point point;
        //    point = (Point)info.GetValue(
        //                       String.Format(CultureInfo.InvariantCulture,
        //                                     "{0}{1}-{2}-{3}",
        //                                     new object[] { entryPoint, orderNumber, objectIndex, i }),
        //                       typeof(Point));
        //    pointArray.Add(point);
        //}
        //base.LoadFromStream(info, orderNumber, objectIndex);
    }

    /// <summary>
    /// Create graphic object used for hit test
    /// </summary>
    protected override void CreateObjects()
    {
        if (AreaPath != null)
            return;

        // Create closed path which contains all polygon vertexes
        AreaPath = new GraphicsPath();

        int x1 = 0, y1 = 0; // previous point

        IEnumerator enumerator = pointArray.GetEnumerator();

        if (enumerator.MoveNext())
        {
            x1 = ((Point)enumerator.Current).X;
            y1 = ((Point)enumerator.Current).Y;
        }

        while (enumerator.MoveNext())
        {
            int x2, y2; // current point
            x2 = ((Point)enumerator.Current).X;
            y2 = ((Point)enumerator.Current).Y;

            AreaPath.AddLine(x1, y1, x2, y2);

            x1 = x2;
            y1 = y2;
        }

        AreaPath.CloseFigure();

        // Create region from the path
        AreaRegion = new Region(AreaPath);
    }

    private void LoadCursor()
    {
        handleCursor = null;//new Cursor(GetType(), "PolyHandle.cur");
    }
    private NodeHandle pathhandle;
    private Subscriber<nm.Path> pathSub;
    //Thread fpThread;
    //bool isClosing;
    Transformer tfer;
    /// <summary>
    /// 订阅更新话题
    /// </summary>
    /// <param name="topic"></param>
    public void SubscribeToPath(string topic)
    {
        lock (this)
        {
            if (pathhandle == null)
                pathhandle = new NodeHandle();
            if (pathSub != null && pathSub.topic != topic)
            {
                pathSub.shutdown();
                pathSub = null;
            }
            if (pathSub != null)
                return;
            if (tfer == null)
                tfer = new Transformer(false);

            Console.WriteLine("Subscribing to path at:= " + topic);
            pathSub = pathhandle.subscribe<nm.Path>(topic, 1, i => owner.BeginInvoke(new Action(() =>
            {
                //if (i.polygon.points.Length == 0) return;

                //if (!tfer.waitForTransform("/map", "/base", i.header.stamp, new m.Duration(new Messages.TimeData(1, 0)), null))
                //    return ;

                //tfer.transformVector("/map",)
                //emTransform ret = new emTransform();
                //if (tfer.lookupTransform("/map", "/base", i.header.stamp, out ret))
                //{

                pointArray.Clear();
                int length = i.poses.Length;
                for (int index = 0;index<i.poses.Length;index++)
                {
                    Point worldPoint = owner.Map2World(new PointF((float)i.poses[index].pose.position.x, (float)i.poses[index].pose.position.y));
                    pointArray.Add(worldPoint);
                }
                //int length = i.polygon.points.Length;
                //for (int index = 0; index < length; index++)
                //{
                //    gm.Point32 p = i.polygon.points[index];

                //    //Stamped<emVector3> stampVector_in = new Stamped<emVector3>(i.header.stamp, i.header.frame_id, new emVector3(p.x,p.y,p.z));
                //    //Stamped<emVector3> stamVector_out = new Stamped<emVector3>();
                //    //tfer.transformVector("map", stampVector_in, ref stamVector_out);

                //    Point worldPoint = owner.Map2World(new PointF(p.x, p.y));
                //    pointArray.Add(worldPoint);
                //}
                //pointArray.Add(pointArray[0]);

                if (length > 0)
                {
                    startPoint = (Point)pointArray[0];
                    endPoint = (Point)pointArray[length - 1];
                }
                Invalidate();
                owner.LastDataUpdateTime = DateTime.Now;
                //owner.Invalidate();
                //}
            })));
        }
    }
}
}
