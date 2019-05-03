using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerLayerTree
{
    class Utility
    {
        public static long GetDirectorySize(String dir)
        {
            long r = 0;
            try
            {
                var di = new System.IO.DirectoryInfo(dir);
                r = di.GetFiles("*.*", System.IO.SearchOption.TopDirectoryOnly).Sum(f => f.Length);
                foreach (var d in di.GetDirectories())
                {
                    if (d.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint) == false)
                    {
                        r += GetDirectorySize(d.FullName);
                    }
                }
            }
            catch { }
            return r;
        }
    }
}
