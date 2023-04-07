using System;
using System.Data.Common;
using SqlKata.Compilers;

namespace BookingCare.Data.DBBase
{
    public interface IDatabaseHelper
    {
        string BeginTry { get; }
        string EndTryBeginCatch { get; }
        string EndCatch { get; }
        string RaisError { get; }
        string TransactionIsolationLevel { get; }
        string RollBackTransaction { get; }
        string BeginTransaction { get; }
        string CommitTransaction { get; }
        string GroupByConcatMaxLen { get; }
        string TableDoesNotExistCode { get; }

        void AddByteArrayParam(DbCommand command, string paramName, byte[] value);
        void AddGuidParam(DbCommand command, string paramName, Guid value);
        void AddDateParam(DbCommand command, string paramName, DateTime value);
        void AddDateTimeParam(DbCommand command, string paramName, DateTime value);
        void AddStringParam(DbCommand command, string paramName, string value);
        void AddDecimalParam(DbCommand command, string paramName, decimal value);
        void AddInt64Param(DbCommand command, string paramName, long value);
        void AddIntParam(DbCommand command, string paramName, int value);
        void AddEnumParam(DbCommand command, string paramName, Enum value);
        void AddParam(DbCommand command, string paramName, object value);
        DbParameter AddIntReturnParam(DbCommand command, string paramName);
        DbParameter AddStringReturnParam(DbCommand command, string paramName);

        DbConnection GetConnection(string connectionString);
        DbCommand GetCommand(DbConnection connection);
        Compiler GetCompiler();
    }
}
