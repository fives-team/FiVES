// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using KIARA;
using System;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Represents a local server.
    /// </summary>
    public interface ILocalServer
    {
        /// <summary>
        /// KIARA service on the local service.
        /// </summary>
        ServiceImplementation Service { get; }

        /// <summary>
        /// Local domain-of-reponsibility.
        /// </summary>
        IDomainOfResponsibility DoR { get; set; }

        /// <summary>
        /// Local domain-of-interest.
        /// </summary>
        IDomainOfInterest DoI { get; set; }

        /// <summary>
        /// Local SyncID.
        /// </summary>
        Guid SyncID { get; }

        /// <summary>
        /// Triggered when the local DoI has changed.
        /// </summary>
        event EventHandler DoIChanged;

        /// <summary>
        /// Triggered when the local DoR has changed.
        /// </summary>
        event EventHandler DoRChanged;
    }
}
