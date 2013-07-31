using System;
using System.Data;
using NHibernate.Dialect;

namespace Persistence
{
    public class MySQLGuid32 :MySQL5Dialect
    {
        public MySQLGuid32 ()
        {
            RegisterColumnType(DbType.Guid, "CHAR(36)");
        }
    }
}

