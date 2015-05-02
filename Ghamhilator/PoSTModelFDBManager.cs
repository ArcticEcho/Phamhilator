using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonFx.Json;



namespace Ghamhilator
{
    public class PoSTModelFDBManager
    {
        private readonly string modelFile;

        public static readonly string root = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        public static readonly string modelDir = Path.Combine(root, "PoST Models");



        public PoSTModelFDBManager(string modelID)
        {
            var path = Path.Combine(root, modelDir, modelID);

            if (!File.Exists(path))
            {
                var dirPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                File.Create(path).Dispose();
            }

            modelFile = path;
        }



        public PoSTModel LoadModel()
        {
            var data = File.ReadAllText(modelFile);
            var obj = new JsonReader().Read<PoSTModel>(data);

            return obj;
        }

        public void UpdateModel(PoSTModel model)
        {
            var json = new JsonWriter().Write(model);
            File.WriteAllText(modelFile, json);
        }
    }
}
