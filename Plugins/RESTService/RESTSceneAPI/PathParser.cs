// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RESTServicePlugin
{
    public class PathParser
    {
        private static PathParser instance = new PathParser();

        public static PathParser Instance { get { return instance; } }

        public string GetFirstPathObject(string requestPath)
        {
            int delimiterIndex = requestPath.IndexOf("/");
            return requestPath.Substring(0, delimiterIndex);
        }
        public string GetRemainingPath(string requestPath)
        {
            int delimiterIndex = requestPath.IndexOf("/");
            return requestPath.Substring(delimiterIndex + 1, requestPath.Length - delimiterIndex - 1);
        }
    }
}
