using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerLayerTree
{
    class Layer
    {
        public string Directory { get; }
        public string ID { get; set; }
        public string Description { get; set; }
        public string LayerSize { get; set; }

        public Layer Parent { get; set; }
        public List<Layer> Children { get; set; }

        public void AddChild(Layer child)
        {
            if (Children.Exists(x => String.Equals(x.Directory, child.Directory, StringComparison.OrdinalIgnoreCase)) == false)
            {
                Children.Add(child);
            }
        }

        public void LoadLayerSize()
        {
            var dirSize = Utility.GetDirectorySize(Directory);
            LayerSize = String.Format("[{0:0.##} GB]", Convert.ToDouble(dirSize) / 1024.0 / 1024.0 / 1024.0);
        }

        protected Layer()
        {

        }

        public Layer(string dirname)
        {
            Children = new List<Layer>();

            this.Directory = dirname;
        }
    }
}
