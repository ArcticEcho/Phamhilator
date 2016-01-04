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
    public static class UserAccess
    {
        static public int[] Owners
        {
            get
            {
                return new[]
                {
                    227577,  // Sam (MSE)
                    2246344, // Sam (SO)
                    266094,  // Uni (MSE)
                    3622940, // Uni (SO)
                    229438,  // Fox (MSE)
                    2619912, // Fox (SO)
                    194047,  // Jan (MSE)
                    245360,  // Pat (MSE)
                    202832,  // Moo (MSE)
                };
            }
        }
    }
}
