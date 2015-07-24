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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RESTServicePlugin
{
    class RESTSceneAPI : RequestHandler
    {
        public override string Path
        {
            get { return "/entities"; }
        }

        public override string ContentType
        {
            get { return "text/xml"; }
        }

        XmlDocument responseDocument;

        protected override RequestResponse HandleGET(string requestPath)
        {
            var response = new RequestResponse();
            responseDocument = new XmlDocument();
            GetResolver getResolver = new GetResolver(new SceneWriter(responseDocument));
            var responseObject = getResolver.WriteSceneOrGetEntity(requestPath.Trim('/').TrimEnd('/'));
            if (responseObject.GetType() == typeof(string))
                response.SetResponseBuffer((string)responseObject);
            else
            {
                responseDocument.AppendChild((XmlElement)responseObject);
                response.SetResponseBuffer(XmlToString(responseDocument));
            }
            response.ReturnCode = 200;
            return response;
        }

        protected override RequestResponse HandlePOST(string requestPath, string content)
        {
            var response = new RequestResponse();
            responseDocument = new XmlDocument();
            responseDocument.AppendChild(new PutPostResolver(responseDocument).AddEntity(requestPath, content));
            response.SetResponseBuffer(XmlToString(responseDocument));
            response.ReturnCode = 201;
            return response;
        }

        protected override RequestResponse HandlePUT(string requestPath, string content)
        {
            var response = new RequestResponse();
            responseDocument = new XmlDocument();

            try
            {
                responseDocument.AppendChild(
                    new PutPostResolver(responseDocument).UpdateEntity(requestPath.TrimStart('/').TrimEnd('/'), content)
                );
                response.ReturnCode = 200;
                response.SetResponseBuffer(XmlToString(responseDocument));
            }
            catch (EntityNotFoundException)
            {
                response.ReturnCode = 404;
                response.SetResponseBuffer("Entity with given GUID is not present in World");
            }
            catch (FormatException e)
            {
                response.ReturnCode = 400;
                response.SetResponseBuffer(e.Message);
            }

            return response;
        }

        protected override RequestResponse HandleDELETE(string requestPath)
        {
            RequestResponse response = new RequestResponse();
            try
            {
                bool deleted = new DeleteResolver().DeleteEntity(requestPath.Trim('/').TrimEnd('/'));
                response.ReturnCode = deleted ? 204 : 202;
            }
            catch(InvalidOperationException e)
            {
                response.ReturnCode = 501;
                response.SetResponseBuffer("An error occurred when trying to delete an entity: "
                    + e.Message);
            }
            return response;
        }

        private string XmlToString(XmlDocument xml)
        {
            var stringWrite = new StringWriter();
            XmlWriter xmlWriter = XmlWriter.Create(stringWrite);
            xml.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWrite.GetStringBuilder().ToString();
        }
    }
}
