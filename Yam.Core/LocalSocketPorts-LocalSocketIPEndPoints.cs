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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Phamhilator.Yam.Core
{
    public enum LocalSocketPort
    {
        YamToAll = 60000,
        PhamToYam = 60001,
        GhamToYam = 60002
    }

    public static class LocalSocketIPEndPoints
    {
        public static IPAddress MulticastAddress
        {
            get { return IPAddress.Parse("239.0.0.222"); }
        }

        public static IPEndPoint YamToAll
        {
            get { return new IPEndPoint(IPAddress.Any, (int)LocalSocketPort.YamToAll); }
        }

        public static IPEndPoint PhamToYam
        {
            get { return new IPEndPoint(IPAddress.Any, (int)LocalSocketPort.PhamToYam); }
        }

        public static IPEndPoint GhamToYam
        {
            get { return new IPEndPoint(IPAddress.Any, (int)LocalSocketPort.GhamToYam); }
        }
    }
}
