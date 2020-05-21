using System;

namespace CodeSamples.Database
{
    internal class ConnectionSession : IDisposable
    {
        private DatabaseConnection _database;

        public ConnectionSession(DatabaseConnection database)
        {
            _database = database;
        }
                
        #region IDisposable Members

        public void Dispose()
        {
            if (_database != null)
            {
                _database.Close();
                _database = null;
            }
        }

        #endregion
    }
}
