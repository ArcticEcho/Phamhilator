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
using Newtonsoft.Json;

namespace Phamhilator.Pham.Core
{
    public static class TermReader
    {
        public static List<Term> ReadTerms(string filePath, FilterConfig filterType)
        {
            List<JsonTerm> data;
            var terms = new List<Term>();

            try
            {
                data = JsonConvert.DeserializeObject<List<JsonTerm>>(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Can't read file '{0}'. Reason: {1}", filePath, ex.Message), ex);
            }

            if (data == null) { return terms; }

            foreach (var t in data)
            {
                terms.Add(t.ToTerm(filterType));
            }

            return terms;
        }
    }
}
