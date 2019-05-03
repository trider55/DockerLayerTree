using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DockerLayerTree
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        LayerCollection layers;

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        protected async void LoadData()
        {
            layers = new LayerCollection();

            await layers.LoadFromDocker();

            LayerCollectionTreeView.LoadTreeView(layers.GetLayers(), treeView1);

            toolStripStatusLabel1.Text = "Loading layer sizes...";
            backgroundWorker1.RunWorkerAsync();
        }

        protected delegate void SafeCallUpdateTreeNodeTextDelegate(TreeNode n, String text);

        protected void UpdateTreeNodeText(TreeNode n, String text)
        {
            n.Text = text;
        }

        protected void ReloadNodeText(TreeNode n)
        {
            if (n == null)
                return;

            var l = (Layer)n.Tag;

            l.LoadLayerSize();

            var newText = LayerCollectionTreeView.GetLayerNodeText(l);

            var d = new SafeCallUpdateTreeNodeTextDelegate(UpdateTreeNodeText);

            treeView1.Invoke(d, new object[] { n, newText });

            foreach (TreeNode c in n.Nodes)
            {
                ReloadNodeText(c);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker1.ReportProgress(0);
            int totals = treeView1.Nodes.Count;
            int done = 0;
            foreach (TreeNode n in treeView1.Nodes)
            {
                ReloadNodeText(n);
                done += 1;
                backgroundWorker1.ReportProgress(Convert.ToInt32(Convert.ToDecimal(done) / totals * 100));
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripStatusLabel1.Text = "---";
        }

        private int DrawText(DrawTreeNodeEventArgs e, string text, int startingXPosition, Color foreColor)
        {
            var s = TextRenderer.MeasureText(text, e.Node.NodeFont);
            TextRenderer.DrawText(e.Graphics, text, e.Node.NodeFont, new Point(e.Node.Bounds.Left + startingXPosition, e.Node.Bounds.Top), foreColor, e.Node.BackColor);

            return s.Width;
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Tag is Layer)
            {
                e.DrawDefault = false;

                Layer l = (Layer)e.Node.Tag;
                int x = 0;
                int w = 0;

                w = DrawText(e, l.ID, x, Color.Blue);
                x = x + w + 10;

                w = DrawText(e, l.LayerSize, x, Color.Black);
                x = x + w + 10;

                w = DrawText(e, l.Description, x, Color.Red);
                x = x + w + 10;

                w = DrawText(e, "Dir: " + System.IO.Path.GetFileName(l.Directory), x, Color.Green);

            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView1.SelectedNode = null;
        }
    }
}
