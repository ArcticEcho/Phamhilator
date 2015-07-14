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
using ServiceStack.Text;

namespace Phamhilator.Yam.Core
{
    public class CueManager
    {
        private const string cuesDataManagerKey = "Cues";
        private const string fSiteDataManagerKey = "Foreign Sites";
        private readonly LocalRequestClient yamClient;
        private readonly HashSet<string> foreignSites;

        public HashSet<Cue> Cues { get; private set; }



        public CueManager(ref LocalRequestClient client)
        {
            if (client == null) { throw new ArgumentNullException("client"); }

            yamClient = client;
            Cues = new HashSet<Cue>();
            foreignSites = new HashSet<string>();

            if (!client.DataExists(client.Caller, cuesDataManagerKey))
            {
                return;
            }

            var cueJson = client.RequestData(client.Caller, cuesDataManagerKey);
            Cues = JsonSerializer.DeserializeFromString<HashSet<Cue>>(cueJson);

            if (!client.DataExists(client.Caller, fSiteDataManagerKey))
            {
                return;
            }

            var fSiteJson = client.RequestData(client.Caller, fSiteDataManagerKey);
            foreignSites = JsonSerializer.DeserializeFromString<HashSet<string>>(fSiteJson);
        }

        ~CueManager()
        {
            UpdateSavedCues();
        }



        public void RegisterTruePositive(Dictionary<CueType, HashSet<Cue>> cues)
        {
            throw new NotImplementedException();
        }

        public void RegisterFalsePositive(Dictionary<CueType, HashSet<Cue>> cues)
        {
            throw new NotImplementedException();
        }

        public void AddForeignSite(string site)
        {
            throw new NotImplementedException();
        }

        public void RemoveForeignSite(string site)
        {
            throw new NotImplementedException();
        }

        public void AddCue(Cue cue)
        {
            if (cue == null) { throw new ArgumentNullException("cue"); }

            Cues.Add(cue);

            UpdateSavedCues();
        }

        public void RemoveCue(CueType type, string cuePattern)
        {
            if (cuePattern == null) { throw new ArgumentNullException("cuePattern"); }

            Cues.RemoveWhere(c => c.Type == type && c.Pattern.ToString() == cuePattern);

            UpdateSavedCues();
        }

        public Dictionary<CueType, HashSet<Cue>> FindCues(string text, string site)
        {
            if (String.IsNullOrEmpty(text))
            {
                throw new ArgumentException("'text' cannot be null or empty.", "text");
            }

            var foundCues = new Dictionary<CueType, HashSet<Cue>>();

            foreach (CueType cueType in Enum.GetValues(typeof(CueType)))
            {
                var typeCues = Cues.Where(c => c.Type == cueType);
                var typeCuesFound = new HashSet<Cue>();

                foreach (var cue in typeCues)
                {
                    if (cue.GetRegex().IsMatch(text))
                    {
                        cue.Found++;
                        typeCuesFound.Add(cue);
                    }
                }

                if (typeCuesFound.Count > 0)
                {
                    foundCues[cueType] = typeCuesFound;
                }
            }

            foreach (var cueSet in foundCues)
            foreach (var cue in cueSet.Value)
            {
                Cues.Remove(cue);
                Cues.Add(cue);
            }

            UpdateSavedCues();

            return foundCues;
        }



        private void UpdateSavedCues()
        {
            var json = JsonSerializer.SerializeToString(Cues);
            yamClient.UpdateData(yamClient.Caller, cuesDataManagerKey, json);
        }
    }
}
