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
using System.ServiceModel;

namespace TestingPlugin
{
    /// <summary>
    /// Testing service used to communicate from the FiVES server to the testing process, which has started it.
    /// </summary>
    [ServiceContract()]
    public interface ITestingService
    {
        /// <summary>
        /// Called when the server is ready, which happens when all plugins are loaded.
        /// </summary>
        [OperationContract]
        void NotifyServerReady();
    }
}
