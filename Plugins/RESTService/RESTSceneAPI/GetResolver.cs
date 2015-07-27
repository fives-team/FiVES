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
using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RESTServicePlugin
{
    public class GetResolver
    {
        SceneWriter sceneWriter;

        public GetResolver(SceneWriter writer)
        {
            this.sceneWriter = writer;
        }

        public object WriteSceneOrGetEntity(string requestPath)
        {
            if (requestPath.Length == 0)
                return sceneWriter.WriteScene();
            else
                return WriteEntityOrGetComponent(requestPath);

        }

        public object WriteEntityOrGetComponent(string requestPath)
        {
            if (!requestPath.Contains('/'))
                return sceneWriter.WriteEntity(World.Instance.FindEntity(requestPath));
            string entityGuid = PathParser.Instance.GetFirstPathObject(requestPath);
            string remainingPath = PathParser.Instance.GetRemainingPath(requestPath);
            return WriteComponentOrGetAttribute(entityGuid, remainingPath);
        }

        public object WriteComponentOrGetAttribute(string entityGuid, string remainingRequestPath)
        {
            if (!remainingRequestPath.Contains('/'))
                return sceneWriter.WriteComponent(World.Instance.FindEntity(entityGuid), remainingRequestPath);

            string[] componentAndAttribute = remainingRequestPath.Split('/');
            return sceneWriter.WriteAttributeValue(World.Instance.FindEntity(entityGuid),
                componentAndAttribute[0], componentAndAttribute[1]);
        }

    }
}
