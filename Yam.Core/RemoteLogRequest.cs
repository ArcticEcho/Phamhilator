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





namespace Phamhilator.Yam.Core
{
    /// <summary>
    /// See https://github.com/ArcticEcho/Phamhilator/wiki/Yam-API for more details.
    /// </summary>
    public class RemoteLogRequest
    {
        public string PostType { get; set; }
        public string Site { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Score { get; set; }
        public string CreatedAfter { get; set; }
        public string CreatedBefore { get; set; }
        public string EntryAddedAfter { get; set; }
        public string EntryAddedBefore { get; set; }
        public string AuthorName { get; set; }
        public string AuthorRep { get; set; }
        public string AuthorNetworkID { get; set; }



        public RemoteLogRequest()
        {
            // Dear future maintainer, don't forget to update these values once we reach 2200.
            CreatedAfter = "1970-1-1";
            CreatedBefore = "2200-1-1";
            EntryAddedAfter = "1970-1-1";
            EntryAddedBefore = "2200-1-1";
        }
    }
}
