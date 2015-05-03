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





namespace Yam.Core
{
    public abstract class Post
    {
        public string Title { get; protected set; }
        public string AuthorName { get; protected set; }
        public string AuthorLink { get; protected set; }
        public string Url { get; protected set; }
        public string Site { get; protected set; }
        public string Body { get; protected set; }
        public int Score { get; protected set; }
        public int AuthorRep { get; protected set; }
    }
}
