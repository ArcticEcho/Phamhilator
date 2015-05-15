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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phamhilator.Yam.Core;
using Newtonsoft.Json;

namespace Phamhilator.Pham.UI
{
    public class Classifier
    {
        private readonly LocalRequestClient localClient;
        private readonly object blackHooks;
        private readonly object whiteHooks;
        private readonly object blackNet;
        private readonly object whiteNet;



        public Classifier(LocalRequestClient localClient)
        {
            if (localClient == null) { throw new ArgumentNullException("localClient"); }
            this.localClient = localClient;

            var blkHkData = localClient.RequestData("pham", "Black Hook ");
            blackHooks = JsonConvert.DeserializeObject<object>(blkHkData);
        }



        public ClassificationResults Classify(Post post)
        {



            return null;
        }



        private void CheckBlackHooks(Post post)
        {

        }

        private void CheckWhiteHooks(Post post)
        {

        }

        private void CheckBlackNet(Post post)
        {

        }

        private void CheckWhiteNet(Post post)
        {

        }
    }
}
