using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using YamlDotNet.RepresentationModel;

using tf.net;

using Ros_CSharp;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;


using Messages.roscpp_tutorials;

namespace MapServer
{
    public class Map_server
    {
        public Map_server(NodeHandle nh)
        {
            this.rosNodeHandle = nh;
            map_resp_ = new nm.GetMap.Response();
        }
        double resolution;
        string mapfname;
        double[] origin;
        int negate;
        double occ_th, free_th;
        MapMode mode = MapMode.TRINARY;
        string fram_id="map";
        public void LoadConfig(string filefullname)
        {

                using (StreamReader reader = File.OpenText(filefullname))
                {
                    YamlStream yaml = new YamlStream();
                    yaml.Load(reader);
                    var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                    resolution = Convert.ToDouble(mapping.Children[new YamlScalarNode("resolution")].ToString());
                    negate = Convert.ToInt32(mapping.Children[new YamlScalarNode("negate")].ToString());
                    occ_th = Convert.ToDouble(mapping.Children[new YamlScalarNode("occupied_thresh")].ToString());
                    free_th = Convert.ToDouble(mapping.Children[new YamlScalarNode("free_thresh")].ToString());
                try
                {
                    string modeS = "";
                    modeS = ((YamlScalarNode)mapping.Children[new YamlScalarNode("mode")]).Value;
                    if (modeS == "trinary")
                        mode = MapMode.TRINARY;
                    else if (modeS == "scale")
                        mode = MapMode.SCALE;
                    else if (modeS == "raw")
                        mode = MapMode.RAW;
                    else
                        return ;
                }
                catch(Exception ex)
                {

                }
                    origin = new double[3];
                    origin[0] = Convert.ToDouble(mapping.Children[new YamlScalarNode("origin")][0].ToString());
                    origin[1] = Convert.ToDouble(mapping.Children[new YamlScalarNode("origin")][1].ToString());
                    origin[2] = Convert.ToDouble(mapping.Children[new YamlScalarNode("origin")][2].ToString());

                    mapfname= ((YamlScalarNode)mapping.Children[new YamlScalarNode("image")]).Value;

                }

            PGM.PGM img = new PGM.PGM(Path.Combine(Path.GetDirectoryName(filefullname), mapfname));
            map_resp_.map = new nm.OccupancyGrid();
            map_resp_.map.info = new nm.MapMetaData();
            map_resp_.map.info.width = (uint)img.Width;
            map_resp_.map.info.height = (uint)img.Length;
            map_resp_.map.info.resolution = (float)resolution;

            map_resp_.map.info.origin = new gm.Pose();
            map_resp_.map.info.origin.position = new gm.Point();
            map_resp_.map.info.origin.position.x = origin[0];
            map_resp_.map.info.origin.position.y = origin[1];
            map_resp_.map.info.origin.position.z = 0;
            

            tf.net.emQuaternion q=emQuaternion.FromRPY(new emVector3(0,0,origin[2]));
            map_resp_.map.info.origin.orientation = new gm.Quaternion();
            map_resp_.map.info.origin.orientation.x = 0;
            map_resp_.map.info.origin.orientation.y = 0;
            map_resp_.map.info.origin.orientation.z = 0;
            map_resp_.map.info.origin.orientation.w = 1;

            map_resp_.map.data = new sbyte[map_resp_.map.info.width * map_resp_.map.info.height];

            int rowstride = img.Width;
            byte thevalue=0;
            double occ;
            for(int j=0;j<map_resp_.map.info.height;j++)
            {
                for(int i=0;i<map_resp_.map.info.width;i++)
                {
                    double color_avg=img.Data[j*rowstride+i];
                    if(!(negate==0))
                    {
                        color_avg = 255 - color_avg;
                    }
                    if(mode==MapMode.RAW)
                    {
                        thevalue =(byte)color_avg;
                        map_resp_.map.data[(map_resp_.map.info.width * (map_resp_.map.info.height - j - 1) + i)] = (sbyte)thevalue;
                        continue;
                    }
                    occ = (255 - color_avg) / 255.0;
                    if (occ > occ_th)
                        thevalue = +100;
                    else if (occ < free_th)
                        thevalue = 0;
                    else if (mode == MapMode.TRINARY)
                        thevalue = 255;
                    else
                    {
                        double ratio = (occ - free_th) / (occ_th - free_th);
                        thevalue = (byte)(99 * ratio);
                    }
                    map_resp_.map.data[map_resp_.map.info.width * (map_resp_.map.info.height - j - 1) + i] = (sbyte)thevalue;
                }
            }
            map_resp_.map.info.map_load_time = ROS.GetTime();
            map_resp_.map.header = new m.Header();
            map_resp_.map.header.frame_id = fram_id;
            map_resp_.map.header.stamp = ROS.GetTime();
            mete_data_message_ = map_resp_.map.info;

          
        }

        private bool mapCallback(nm.GetMap.Request req,ref nm.GetMap.Response resp)
        {
            resp = map_resp_;
            return true;
        }
        ServiceServer service;
        NodeHandle rosNodeHandle;
        nm.MapMetaData mete_data_message_;
        nm.GetMap.Response map_resp_;
        Publisher<nm.OccupancyGrid> map_pub;
        Publisher<nm.MapMetaData> metadata_pub;


        private ServiceServer server;

        private bool addition(TwoInts.Request req, ref TwoInts.Response resp)
        {
            resp.sum = req.a + req.b;
            long sum = resp.sum;
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    math.Content = "" + req.a + " + " + req.b + " = ??\n" + sum;
            //}));
            return true;
        }


        public void Run()
        {

            
            metadata_pub = rosNodeHandle.advertise<nm.MapMetaData>("map_metadata", 1,true);
            if (metadata_pub != null)
                metadata_pub.publish(mete_data_message_);
            else
                throw new Exception("map_metadata话题发布失败！");
            map_pub = rosNodeHandle.advertise<nm.OccupancyGrid>("map", 1,true);
            if (map_pub != null)
                map_pub.publish(map_resp_.map);
            else
                throw new Exception("map话题发布失败！");

            server = rosNodeHandle.advertiseService<TwoInts.Request, TwoInts.Response>("/add_two_ints", addition);
            //可能rosnet 框架问题，服务不能够发布，后面解决
            service = rosNodeHandle.advertiseService<nm.GetMap.Request, nm.GetMap.Response>("/static_map", mapCallback);
        }
        public void Stop()
        {
            service.shutdown();
            metadata_pub.shutdown();
            map_pub.shutdown();
            server.shutdown();
        }
    }
    public enum MapMode { TRINARY,SCALE,RAW};
}
