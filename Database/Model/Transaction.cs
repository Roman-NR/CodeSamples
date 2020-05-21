using System;

namespace CodeSamples.Database
{
    internal class Transaction : IDisposable
    {
        private DatabaseConnection _database;
        private string _name;

        public Transaction(DatabaseConnection database, string name)
        {
            _database = database;
            _name = name;
            _database.BeginTransaction(name);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_name != null)
                Rollback();
        }

        #endregion

        public string Name
        {
            get { return _name; }
        }

        public override string ToString()
        {
            return Name;
        }

        public void Commit()
        {
            if (_name == null)
                throw new ApplicationException(Properties.Resources.ClosedTransactionCommitCall);

            _database.CommitTransaction();
            _name = null;
        }

        public void Rollback()
        {
            if (_name == null)
                throw new ApplicationException(Properties.Resources.ClosedTransactionRollbackCall);

            _database.RollbackTransaction(_name);
            _name = null;
        }
    }
}
