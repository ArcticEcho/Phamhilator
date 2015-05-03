/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonFx.Json;

namespace Phamhilator.Gham
{
    public class PoSTModelFFDBManager
    {
        private readonly string modelFile;

        public static readonly string root = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        public static readonly string modelDir = Path.Combine(root, "PoST Models");



        public PoSTModelFFDBManager(string modelID)
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
