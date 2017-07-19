using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using tf.net;

using Ros_CSharp;
using m = Messages.std_msgs;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;
using sm = Messages.sensor_msgs;

namespace MapServer
{
    public class Map_saver
    {
        //public Map_saver(NodeHandle nh)
        //{
        //    this.rosNode = nh;
        //}
        //bool isSavedMap = false;
        //NodeHandle rosNode;
        /// <summary>
        /// 保存地图数据
        /// </summary>
        /// <param name="mapName"></param>
        /// <param name="path"></param>
        public static void SaveMap(string mapName,string path,nm.OccupancyGrid m)
        {
            //isSavedMap = false;
            //int count = 0;
            //Subscriber<nm.OccupancyGrid> mapSub = rosNode.subscribe<nm.OccupancyGrid>("/map", 1, (m) => 
            //{

                string mapdatafile = mapName+".pgm";
                using (BinaryWriter file = new BinaryWriter(File.Open(System.IO.Path.Combine(path,mapdatafile), FileMode.Create), Encoding.ASCII))
                {
                    file.Write(string.Format("P5\n# CREATOR:Map_saver.cs {0:f3} m/pix\n{1} {2}\n255\n", m.info.resolution, m.info.width, m.info.height).ToCharArray());
                    for (uint y = 0; y < m.info.height; y++)
                    {
                        for (uint x = 0; x < m.info.width; x++)
                        {
                            uint i = x + (m.info.height - y - 1) * m.info.width;
                            if (m.data[i] == 0)
                                file.Write((byte)254);
                            else if (m.data[i] == +100)
                            {
                                file.Write((byte)000);
                            }
                            else
                                file.Write((byte)205);
                        }
                    }
                    file.Flush();
                    file.Close();

                    string mapmetadatafile = mapName + ".yaml";
                    using (BinaryWriter metafile = new BinaryWriter(File.Open(Path.Combine(path, mapmetadatafile), FileMode.Create), Encoding.ASCII))
                    {
                        emQuaternion orientation = new emQuaternion(m.info.origin.orientation);
                        emVector3 rpy= orientation.getRPY();
                        metafile.Write(string.Format("image: {0}\nresolution: {1:f}\norigin: [{2},{3},{4}]\nnegate: 0\noccupied_thresh: 0.65\nfree_thresh: 0.196\n\n",
                            mapdatafile, m.info.resolution, m.info.origin.position.x, m.info.origin.position.y, rpy.z).ToCharArray());
                        metafile.Flush();
                        metafile.Close();
                        
                    }
                    //isSavedMap = true;
                }
            //});
            //while(!isSavedMap)
            //{
            //    if(count++>20)
            //    {
            //        throw new Exception("保存地图超时！");
            //        break;
            //    }
            //    System.Threading.Thread.Sleep(500);
            //}
            //mapSub.shutdown();
        }
    }
}
