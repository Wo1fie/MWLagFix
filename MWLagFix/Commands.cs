/*
    This file is part of MWLagFix.

    MWLagFix is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MWLagFix is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MWLagFix.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;

namespace MWLagFix
{
    public class Commands : Dictionary<object, Action<object>>
    {
        public bool Process(object key, object args = null)
        {
            if (base.ContainsKey(key))
            {
                base[key].Invoke(args);
                return true;
            }
            return false;
        }
    }
}
