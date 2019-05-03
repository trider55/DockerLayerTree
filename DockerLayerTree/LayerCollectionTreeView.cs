using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DockerLayerTree
{
    class LayerCollectionTreeView
    {
        public static string GetLayerNodeText(Layer l)
        {
            return String.Format("{0}  {1} {2} - {3}",
                l.ID,
                l.LayerSize,
                l.Description,
                System.IO.Path.GetFileName(l.Directory));
        }

        protected static void LoadChildren(TreeNode parentNode, Layer parentLayer)
        {
            if (parentLayer == null)
                return;

            foreach (Layer l in parentLayer.Children)
            {
                var node = parentNode.Nodes.Add(GetLayerNodeText(l));
                node.Tag = l;
                LoadChildren(node, l);
            }
        }

        public static void LoadTreeView(IEnumerable<Layer> layers, TreeView treeView)
        {
            treeView.Nodes.Clear();

            foreach (Layer l in layers.Where(l => l.Parent == null))
            {
                var node = treeView.Nodes.Add(GetLayerNodeText(l));
                node.Tag = l;
                LoadChildren(node, l);
            }

            treeView.ExpandAll();
        }
    }
}
