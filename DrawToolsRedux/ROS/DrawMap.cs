using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.IO;

using Ros_CSharp;
using d = System.Drawing;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;
namespace DrawTools.ROS
{
    /// <summary>
    /// Map graphic object
    /// </summary>
    public class DrawMap:DrawObject
    {
        private Rectangle rectangle;
        private Bitmap _image;
        // this holds the original image unscaled
        private Bitmap _originalImage;

        public Bitmap TheImage
        {
            get { return _image; }
            set
            {
                _originalImage = value;
                ResizeImage(rectangle.Width, rectangle.Height);
            }
        }

        private const string entryRectangle = "Rect";
        private const string entryImage = "Image";
        private const string entryImageOriginal = "OriginalImage";


        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            //DrawMap drawImage = new DrawMap();
            //drawImage._image = _image;
            //drawImage._originalImage = _originalImage;
            //drawImage.rectangle = rectangle;

            //FillDrawObjectFields(drawImage);
            return null;
        }

        protected Rectangle Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }
        DrawArea owner;
        public DrawMap(DrawArea owner)
        {
            SetRectangle(0, 0, 1, 1);
            Initialize();
            this.owner = owner;
        }

        public DrawMap(int x, int y,DrawArea owner)
        {
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = 1;
            rectangle.Height = 1;
            Initialize();
            this.owner = owner;
        }

        public DrawMap(int x, int y, Bitmap image,DrawArea owner)
        {
            rectangle.X = x;
            rectangle.Y = y;
            _image = (Bitmap)image.Clone();
            SetRectangle(rectangle.X, rectangle.Y, image.Width, image.Height);
            Center = new Point(x + (image.Width / 2), y + (image.Height / 2));
            TipText = String.Format("Image Center @ {0}, {1}", Center.X, Center.Y);
            Initialize();
            this.owner = owner;
        }

        /// <summary>
        /// Draw image
        /// </summary>
        /// <param name="g"></param>
        public override void Draw(Graphics g)
        {
            // Get existing World transformation
            //atrix mSave = g.Transform;

            //Matrix m = mSave.Clone();

            //Point origPoint_world = owner.Map2World(origin);
            //origPoint_world.Offset(-(int)MapHeight, -(int)mapWidth);
            if (_image != null)
            {
                g.DrawImage(_image, rectangle.Location);
                // Draw map original point
                g.DrawLine(Pens.Red, new Point(0, -10), new Point(0, 10));
                g.DrawLine(Pens.Red, new Point(-10, 0), new Point(10, 0));
            }
            /*if (Rotation != 0)
            {
                Matrix m = mSave.Clone();
                m.RotateAt(Rotation, new PointF(rectangle.Left + (rectangle.Width / 2), rectangle.Top + (rectangle.Height / 2)), MatrixOrder.Append);
                g.Transform = m;
            }
            if (_image == null)
            {
                Pen p = new Pen(Color.Black, -1f);
                g.DrawRectangle(p, rectangle);
            }
            else
                g.DrawImage(_image, new Point(rectangle.X, rectangle.Y));*/
            // Restore World transformation
            //g.Transform = mSave;
        }

        protected void SetRectangle(int x, int y, int width, int height)
        {
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = width;
            rectangle.Height = height;
        }

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get { return 8; }
            //get { return 0; }
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override Point GetHandle(int handleNumber)
        {
            int x, y, xCenter, yCenter;

            xCenter = rectangle.X + rectangle.Width / 2;
            yCenter = rectangle.Y + rectangle.Height / 2;
            x = rectangle.X;
            y = rectangle.Y;

            switch (handleNumber)
            {
                case 1:
                    x = rectangle.X;
                    y = rectangle.Y;
                    break;
                case 2:
                    x = xCenter;
                    y = rectangle.Y;
                    break;
                case 3:
                    x = rectangle.Right;
                    y = rectangle.Y;
                    break;
                case 4:
                    x = rectangle.Right;
                    y = yCenter;
                    break;
                case 5:
                    x = rectangle.Right;
                    y = rectangle.Bottom;
                    break;
                case 6:
                    x = xCenter;
                    y = rectangle.Bottom;
                    break;
                case 7:
                    x = rectangle.X;
                    y = rectangle.Bottom;
                    break;
                case 8:
                    x = rectangle.X;
                    y = yCenter;
                    break;
            }
            return new Point(x, y);
        }

        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override int HitTest(Point point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (PointInObject(point))
                return 0;
            return -1;
        }

        protected override bool PointInObject(Point point)
        {
            return rectangle.Contains(point);
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return Cursors.Default;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="handleNumber"></param>
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            int left = Rectangle.Left;
            int top = Rectangle.Top;
            int right = Rectangle.Right;
            int bottom = Rectangle.Bottom;

            switch (handleNumber)
            {
                case 1:
                    left = point.X;
                    top = point.Y;
                    break;
                case 2:
                    top = point.Y;
                    break;
                case 3:
                    right = point.X;
                    top = point.Y;
                    break;
                case 4:
                    right = point.X;
                    break;
                case 5:
                    right = point.X;
                    bottom = point.Y;
                    break;
                case 6:
                    bottom = point.Y;
                    break;
                case 7:
                    left = point.X;
                    bottom = point.Y;
                    break;
                case 8:
                    left = point.X;
                    break;
            }
            Dirty = true;
            SetRectangle(left, top, right - left, bottom - top);
            ResizeImage(rectangle.Width, rectangle.Height);
        }

        protected void ResizeImage(int width, int height)
        {
            //if (_originalImage != null)
            //{
            //    Bitmap b = new Bitmap(_originalImage, new Size(width, height));
            //    _image = (Bitmap)b.Clone();
            //    b.Dispose();
            //}
        }

        public override bool IntersectsWith(Rectangle rectangle)
        {
            return Rectangle.IntersectsWith(rectangle);
        }

        /// <summary>
        /// Move object
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public override void Move(int deltaX, int deltaY)
        {
            rectangle.X += deltaX;
            rectangle.Y += deltaY;
            Dirty = true;
        }

        public override void Dump()
        {
            base.Dump();

            Trace.WriteLine("rectangle.X = " + rectangle.X.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Y = " + rectangle.Y.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Width = " + rectangle.Width.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Height = " + rectangle.Height.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Normalize rectangle
        /// </summary>
        public override void Normalize()
        {
            rectangle = DrawRectangle.GetNormalizedRectangle(rectangle);
        }

        /// <summary>
        /// Save object to serialization stream
        /// </summary>
        /// <param name="info"></param>
        /// <param name="orderNumber"></param>
        /// <param name="objectIndex"></param>
        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            //info.AddValue(
            //    String.Format(CultureInfo.InvariantCulture,
            //                  "{0}{1}-{2}",
            //                  entryRectangle, orderNumber, objectIndex),
            //    rectangle);
            //info.AddValue(
            //    String.Format(CultureInfo.InvariantCulture,
            //                  "{0}{1}-{2}",
            //                  entryImage, orderNumber, objectIndex),
            //    _image);
            //info.AddValue(
            //    String.Format(CultureInfo.InvariantCulture,
            //                  "{0}{1}-{2}",
            //                  entryImageOriginal, orderNumber, objectIndex),
            //    _originalImage);

            base.SaveToStream(info, orderNumber, objectIndex);
        }

        /// <summary>
        /// Load object from serialization stream
        /// </summary>
        /// <param name="info"></param>
        /// <param name="orderNumber"></param>
        /// <param name="objectIndex"></param>
        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            //rectangle = (Rectangle)info.GetValue(
            //                        String.Format(CultureInfo.InvariantCulture,
            //                                      "{0}{1}-{2}",
            //                                      entryRectangle, orderNumber, objectIndex),
            //                        typeof(Rectangle));
            //_image = (Bitmap)info.GetValue(
            //                    String.Format(CultureInfo.InvariantCulture,
            //                                  "{0}{1}-{2}",
            //                                  entryImage, orderNumber, objectIndex),
            //                    typeof(Bitmap));
            //_originalImage = (Bitmap)info.GetValue(
            //                            String.Format(CultureInfo.InvariantCulture,
            //                                          "{0}{1}-{2}",
            //                                          entryImageOriginal, orderNumber, objectIndex),
            //                            typeof(Bitmap));

            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        #region Helper Functions
        public static Rectangle GetNormalizedRectangle(int x1, int y1, int x2, int y2)
        {
            if (x2 < x1)
            {
                int tmp = x2;
                x2 = x1;
                x1 = tmp;
            }

            if (y2 < y1)
            {
                int tmp = y2;
                y2 = y1;
                y1 = tmp;
            }
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        public static Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static Rectangle GetNormalizedRectangle(Rectangle r)
        {
            return GetNormalizedRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
        }
        #endregion Helper Functions

        //private float actualResolution;
        private NodeHandle imagehandle;
        private float mapHeight;
        private float mapResolution; //in Meters per Pixel
        private Subscriber<nm.OccupancyGrid> mapSub;
        //private ServiceClient<nm.GetMap> mapServiceClient;
        private float mapWidth;
        /// <summary>
        /// map low-bottom coordinate
        /// </summary>
        private PointF origin;

        //private float originYaw;

        /// <summary>
        ///     54 byte bitmap file header to be stuck on the front of every byte array from the blimp
        /// </summary>
        private byte[] header;

        /// <summary>
        ///     Used to update the header (if it's needed) if the size of the image is different than the one used to make the
        ///     header
        /// </summary>
        private Size lastSize;
        /// <summary>
        ///     Map Resolution in meters per pixel of the actual rendered map
        /// </summary>
        //public float ActualResolution
        //{
        //    get { return actualResolution; }
        //}

        /// <summary>
        ///     Map Resolution in meters per pixel, (As provided in the map topic, not actual rendered resolution)
        /// </summary>
        public float MapResolution
        {
            get { return mapResolution; }
        }

        /// <summary>
        ///     Map height in pixels (This is the map height as provided in the map topic, not the actual rendered height)
        /// </summary>
        public float MapHeight
        {
            get { return mapHeight; }
        }

        /// <summary>
        ///     Map width in pixels (This is the map width as provided in the map topic, not the actual rendered width)
        /// </summary>
        public float MapWidth
        {
            get { return mapWidth; }
        }

        /// <summary>
        ///     Point set as the origin of the map
        /// </summary>
        public PointF Origin
        {
            get { return origin; }
        }

        private string __topic = null;

        /// <summary>
        ///     Map provider topic
        /// </summary>
        //public string Topic
        //{
        //    get { return __topic as string; }
        //    set
        //    {
        //        __topic = value;
        //        SubscribeToMap(__topic);
        //    }
        //}
        /// <summary>
        /// 采用调用服务方式获取地图数据
        /// </summary>
        /// <param name="serviceName"></param>
        public void GetStaticMap(string serviceName)
        {
            lock(this)
            {
                if (imagehandle == null)
                    imagehandle = new NodeHandle();
                //if(mapServiceClient!=null)
                //{
                //    mapServiceClient.shutdown();
                //    mapServiceClient = null;
                //}
                //if (mapServiceClient != null)
                //    return;
                //mapServiceClient = imagehandle.serviceClient<nm.GetMap>(serviceName);
                
                //nm.GetMap srv = new nm.GetMap();
                nm.GetMap.Request req = new Messages.nav_msgs.GetMap.Request();
                nm.GetMap.Response resp = new Messages.nav_msgs.GetMap.Response();

                if(imagehandle.serviceClient<nm.GetMap.Request,nm.GetMap.Response>(serviceName).call(req,ref resp))
                {
                    nm.OccupancyGrid i = resp.map;
                    mapResolution = i.info.resolution;
                    owner.MapResolution = mapResolution;

                    mapHeight = i.info.height;
                    mapWidth = i.info.width;

                    origin = new PointF((float)i.info.origin.position.x, (float)i.info.origin.position.y);
                    Point left_bottomPoint = owner.Map2World(origin);
                    left_bottomPoint.Offset(-(int)MapHeight, -(int)mapWidth);
                    SetRectangle(left_bottomPoint.X, left_bottomPoint.Y, (int)mapHeight, (int)MapWidth);


                    Size size = new Size((int)i.info.width, (int)i.info.height);
                    byte[] data = createRGBA(i.data);
                    UpdateImage(data, size, false);



                    _originalImage = _image;
                    _image.RotateFlip(RotateFlipType.Rotate270FlipNone);

                    data = null;

                    owner.PanX = owner.Width / 2 - (rectangle.Width / 2 + rectangle.Left);
                    owner.PanY = owner.Height / 2 - (rectangle.Height / 2 + rectangle.Top);
                    //owner.Invalidate();
                    owner.LastDataUpdateTime = DateTime.Now;
                }



            }
        }
        DateTime lastUpdateTime;
        nm.OccupancyGrid occupancyGrid;
        public void SaveMap(string filename)
        {
            if(occupancyGrid!=null)
                MapServer.Map_saver.SaveMap(System.IO.Path.GetFileName(filename), System.IO.Path.GetDirectoryName(filename), occupancyGrid);
        }
        /// <summary>
        /// 订阅更新话题
        /// </summary>
        /// <param name="topic"></param>
        public void SubscribeToMap(string topic)
        {
            
            lock (this)
            {
                if (imagehandle == null)
                    imagehandle = new NodeHandle();
                if (mapSub != null && mapSub.topic != topic)
                {
                    mapSub.shutdown();
                    mapSub = null;
                }
                if (mapSub != null)
                    return;
                Console.WriteLine("Subscribing to map at:= " + topic);

                lastUpdateTime = DateTime.Now;

                mapSub = imagehandle.subscribe<nm.OccupancyGrid>(topic, 1, i =>  owner.BeginInvoke(new Action(() =>
                {

                    //if (lastUpdateTime.AddMilliseconds(1000) > DateTime.Now) return;
                    lastUpdateTime = DateTime.Now;

                    mapResolution = i.info.resolution;
                    owner.MapResolution = mapResolution;

                    mapHeight = i.info.height;
                    mapWidth = i.info.width;

                    origin = new PointF((float)i.info.origin.position.x, (float)i.info.origin.position.y);
                    Point left_bottomPoint = owner.Map2World(origin);
                    left_bottomPoint.Offset(-(int)MapHeight, -(int)mapWidth);
                    SetRectangle(left_bottomPoint.X, left_bottomPoint.Y, (int)mapHeight, (int)MapWidth);


                    //if (Width != 0)
                    //    actualResolution = (mapWidth / (float)Width) * mapResolution;
                    //else
                    //    actualResolution = (mapWidth / (float)ActualWidth) * mapResolution;
                    //if (float.IsNaN(actualResolution) || float.IsInfinity(actualResolution))
                    //    actualResolution = 0;
                    //else
                    //{
                    //    MatchAspectRatio();
                    //}

                    
                    Size size = new Size((int)i.info.width, (int)i.info.height);
                    byte[] data = createRGBA(i.data);
                    UpdateImage(data, size, false);



                    _originalImage = _image;
                    _image.RotateFlip(RotateFlipType.Rotate270FlipNone);

                    data = null;
                    if (occupancyGrid == null)
                    {
                        owner.PanX = owner.Width / 2 - (rectangle.Width / 2 + rectangle.Left);
                        owner.PanY = owner.Height / 2 - (rectangle.Height / 2 + rectangle.Top);
                    }
                    this.occupancyGrid = i;
                    //owner.Invalidate();
                    owner.LastDataUpdateTime = DateTime.Now;

                })));
            }
        }
        private byte[] createRGBA(sbyte[] map)
        {
            byte[] image = new byte[4 * map.Length];
            int count = 0;
            foreach (sbyte j in map)
            {
                switch (j)
                {
                    case -1: ///Unkown occupancy, light gray
                        image[count] = 211;
                        image[count + 1] = 211;
                        image[count + 2] = 211;
                        image[count + 3] = 0xFF;
                        break;
                    case 100: //100% prob of occupancy, dark gray
                        image[count] = 105;
                        image[count + 1] = 105;
                        image[count + 2] = 105;
                        image[count + 3] = 0xFF;
                        break;
                    case 0: //0% prob of occupancy, White
                        image[count] = 255;
                        image[count + 1] = 255;
                        image[count + 2] = 255;
                        image[count + 3] = 0xFF;
                        break;
                    default: //Any other case. (red?)
                        image[count] = 255;
                        image[count + 1] = 0;
                        image[count + 2] = 0;
                        image[count + 3] = 0xFF;
                        break;
                }
                count += 4;
            }
            return image;
        }

        public void UpdateImage(byte[] data, Size size, bool hasHeader, string encoding = null)
        {
            //Console.WriteLine(1 / DateTime.Now.Subtract(wtf).TotalSeconds);
            //wtf = DateTime.Now;
            if (hasHeader)
            {
                UpdateImage(data);
                return;
            }

            if (data != null)
            {
                byte[] correcteddata;
                switch (encoding)
                {
                    case "mono16":
                        correcteddata = new byte[(int)Math.Round(3d * data.Length / 2d)];
                        for (int i = 0, ci = 0; i < data.Length; i += 2, ci += 3)
                        {
                            ushort realDepth = (ushort)((data[i] << 8) | (data[i + 1]));
                            byte pixelcomponent = (byte)Math.Floor(realDepth / 255d);
                            correcteddata[ci] = correcteddata[ci + 1] = correcteddata[ci + 2] = pixelcomponent;
                        }
                        break;
                    case "16UC1":
                        {
                            correcteddata = new byte[(int)Math.Round(3d * data.Length / 2d)];
                            int[] balls = new int[(int)Math.Round(data.Length / 2d)];
                            int maxball = 0;
                            int minball = int.MaxValue;
                            for (int i = 0, ci = 0; i < data.Length; i += 2, ci += 3)
                            {
                                balls[i / 2] = (data[i + 1] << 8) | (data[i]);
                                if (balls[i / 2] > maxball)
                                    maxball = balls[i / 2];
                                if (balls[i / 2] < minball)
                                    minball = balls[i / 2];
                            }
                            for (int i = 0, ci = 0; i < balls.Length; i++, ci += 3)
                            {
                                byte intensity = (byte)Math.Round((maxball - (255d * balls[i] / (maxball - minball))));
                                correcteddata[ci] = correcteddata[ci + 1] = correcteddata[ci + 2] = intensity;
                            }
                        }
                        break;
                    case "mono8":
                    case "bayer_grbg8":
                        correcteddata = new byte[3 * data.Length];
                        for (int i = 0, ci = 0; i < data.Length; i += 1, ci += 3)
                        {
                            correcteddata[ci] = correcteddata[ci + 1] = correcteddata[ci + 2] = data[i];
                        }
                        break;
                    case "32FC1":
                        {
                            correcteddata = new byte[(int)Math.Round(3d * data.Length / 4d)];
                            for (int i = 0, ci = 0; i < data.Length; i += 4, ci += 3)
                            {
                                correcteddata[ci] = correcteddata[ci + 1] = correcteddata[ci + 2] = (byte)Math.Floor(255d * BitConverter.ToSingle(data, i));
                            }
                        }
                        break;
                    default:
                        correcteddata = data;
                        break;
                }
                // make a bitmap file header if we don't already have one
                if (header == null || size != lastSize)
                    MakeHeader(correcteddata, size);

                // stick it on the bitmap data
                try
                {
                    UpdateImage(concat(header, correcteddata));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                data = null;
                correcteddata = null;
            }
        }
        /// <summary>
        ///     Uses a memory stream to turn a byte array into a BitmapImage via helper method, BytesToImage, then passes the image
        ///     to UpdateImage(BitmapImage)
        /// </summary>
        /// <param name="data">
        /// </param>
        public void UpdateImage(byte[] data)
        {
            if(_image!=null)
            {
                _image.Dispose();
                _image = null;
            }
            //TheImage= DrawMap.BytesToBitmap(data);
            _image = DrawMap.BytesToBitmap(data);

        }
        public static Bitmap BytesToBitmap(byte[] Bytes)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Bytes);
                return new Bitmap((Image)new Bitmap(stream));
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            finally
            {
                stream.Close();
            }
        }
        /// <summary>
        ///     makes a file header with appropriate width, height, length, and such for the bytearray + drawing.size sent as
        ///     members of the lazycopypastefixerupper
        /// </summary>
        /// <param name="rawdata">
        ///     The rawdata.
        /// </param>
        /// <param name="size">
        ///     The size.
        /// </param>
        private void MakeHeader(byte[] rawdata, Size size)
        {
            lastSize = size;
            int length = rawdata.Length;
            int wholelength = rawdata.Length + 54;
            int width = (int)size.Width;
            int height = (int)size.Height;

            //Console.WriteLine("width= " + width + "\nheight= " + height + "\nlength=" + length + "\nbpp= " + (length / (width * height)));
            byte bitmask = 255;
            header = new byte[54];
            header[0] = (byte)'B';
            header[1] = (byte)'M';
            header[2] = (byte)(wholelength & bitmask);
            wholelength = wholelength >> 8;
            header[3] = (byte)(wholelength & bitmask);
            wholelength = wholelength >> 8;
            header[4] = (byte)(wholelength & bitmask);
            wholelength = wholelength >> 8;
            header[5] = (byte)(wholelength & bitmask);

            header[10] = 54;
            header[11] = 0;
            header[12] = 0;
            header[13] = 0;

            header[14] = 40;
            header[15] = 0;
            header[16] = 0;
            header[17] = 0;

            header[18] = (byte)(width & bitmask);
            width = width >> 8;
            header[19] = (byte)(width & bitmask);
            width = width >> 8;
            header[20] = (byte)(width & bitmask);
            width = width >> 8;
            header[21] = (byte)(width & bitmask);

            header[22] = (byte)(height & bitmask);
            height = height >> 8;
            header[23] = (byte)(height & bitmask);
            height = height >> 8;
            header[24] = (byte)(height & bitmask);
            height = height >> 8;
            header[25] = (byte)(height & bitmask);
            header[26] = 1;
            header[27] = 0;
            int bpp = ((int)(Math.Floor((double)(rawdata.Length / (size.Width * size.Height))) * 8));
            //Console.WriteLine("BPP = " + bpp);
            header[28] = (byte)bpp;
            header[29] = 0;

            header[30] = 0;
            header[31] = 0;
            header[32] = 0;
            header[33] = 0;

            header[34] = 0x13;
            header[35] = 0x0B;
            header[36] = 0;
            header[37] = 0;

            header[38] = 0x13;
            header[39] = 0x0B;
            header[40] = 0;
            header[41] = 0;

            header[42] = 0;
            header[43] = 0;
            header[44] = 0;
            header[45] = 0;

            header[46] = 0;
            header[47] = 0;
            header[48] = 0;
            header[49] = 0;

            header[50] = (byte)(length & bitmask);
            length = length >> 8;
            header[51] = (byte)(length & bitmask);
            length = length >> 8;
            header[52] = (byte)(length & bitmask);
            length = length >> 8;
            header[53] = (byte)(length & bitmask);
        }

        /// <summary>
        ///     takes 1 byte array, and 1 byte array, and then returns 1 byte array
        /// </summary>
        /// <param name="a">
        /// </param>
        /// <param name="b">
        /// </param>
        /// <returns>
        /// </returns>
        private static byte[] concat(byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length + b.Length];
            Array.Copy(a, result, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }
    }
}
