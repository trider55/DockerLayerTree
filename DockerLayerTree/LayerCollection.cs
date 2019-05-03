using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Docker.DotNet;
using Newtonsoft.Json.Linq;

namespace DockerLayerTree
{
    class LayerCollection
    {
        protected List<Layer> layers;

        public LayerCollection()
        {
            layers = new List<Layer>();
        }

        public Layer GetOrAddByDirectoryName(string dirname)
        {
            Layer r = layers.Where(l => l.Directory == dirname).SingleOrDefault();
            if (r == null)
            {
                r = new Layer(dirname);
                r.ID = DirectoryToSHA256Map[System.IO.Path.GetFileName(dirname)];
                layers.Add(r);
            }
            return r;
        }

        public IEnumerable<Layer> GetLayers()
        {
            return layers;
        }

        protected Dictionary<string, string> DirectoryToSHA256Map;

        protected void LoadLayerDB(String dockerRoot)
        {
            DirectoryToSHA256Map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string layerdbPath = System.IO.Path.Combine(dockerRoot, "image", "windowsfilter", "layerdb", "sha256");
            foreach (var dir in System.IO.Directory.GetDirectories(layerdbPath))
            {
                String layerPhysicalPath = System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "cache-id"));
                String diff = System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "diff"));

                DirectoryToSHA256Map[layerPhysicalPath] = diff;
            }
        }


        public async Task LoadFromDocker()
        {
            var client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();

            var images = await client.Images.ListImagesAsync(
                new Docker.DotNet.Models.ImagesListParameters() { }
            );

            layers.Clear();

            bool layerdbLoaded = false;

            foreach (var i in images)
            {
                var imagename = i.RepoTags[0];
                var imagedetail = await client.Images.InspectImageAsync(imagename);

                if (String.Equals(imagedetail.GraphDriver.Name, "windowsfilter", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var kvp in imagedetail.GraphDriver.Data.Where(d => String.Equals(d.Key, "dir", StringComparison.OrdinalIgnoreCase)))
                    {
                        var dirname = kvp.Value;
                        var jsonFilename = System.IO.Path.Combine(dirname, "layerchain.json");
                        var json = System.IO.File.ReadAllText(jsonFilename);

                        if (layerdbLoaded == false)
                        {
                            var dockerRoot = System.IO.Directory.GetParent(dirname).Parent.FullName;
                            LoadLayerDB(dockerRoot);
                            layerdbLoaded = true;
                        }

                        var imagelayer = GetOrAddByDirectoryName(dirname);
                        imagelayer.Description = i.ID.Substring(7, 14) + ":" + imagename;

                        Object data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                        if (data is JArray)
                        {
                            Layer previousLayer = imagelayer;

                            foreach (JToken element in (JArray)data)
                            {
                                if (element is JValue)
                                {
                                    var layerdirname = Convert.ToString(((JValue)element).Value);

                                    var l = GetOrAddByDirectoryName(layerdirname);

                                    if (previousLayer != null)
                                    {
                                        previousLayer.Parent = l;
                                        l.AddChild(previousLayer);
                                    }

                                    previousLayer = l;
                                }
                            }
                        }
                    }
                }
            }

        }
    }
}
