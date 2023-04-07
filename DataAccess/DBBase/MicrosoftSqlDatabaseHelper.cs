using SqlKata.Compilers;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace BookingCare.Data.DBBase
{
    public class MicrosoftSqlDatabaseHelper : IDatabaseHelper
    {
        public string BeginTry => " BEGIN TRY ";

        public string EndTryBeginCatch => " END TRY BEGIN CATCH ";

        public string EndCatch => " END CATCH ";

        public string RaisError => " DECLARE @Message varchar(MAX) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @State smallint = ERROR_STATE(); RAISERROR(@Message, @Severity, @State); ";

        public string BeginTransaction => " BEGIN TRANSACTION; ";

        public string CommitTransaction => " ; COMMIT TRANSACTION; ";

        public string RollBackTransaction => " ; ROLLBACK TRANSACTION; ";

        public string TransactionIsolationLevel => " SET TRANSACTION ISOLATION LEVEL READ COMMITTED; ";

        public string GroupByConcatMaxLen => " SET SESSION group_concat_max_len = 1000000; ";

        public string TableDoesNotExistCode => "";

        public void AddByteArrayParam(DbCommand command, string paramName, byte[] value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarBinary, Value = value }; ;

            command.Parameters.Add(param);
        }

        public void AddGuidParam(DbCommand command, string paramName, Guid value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Binary, Value = value.ToByteArray() };

            command.Parameters.Add(param);
        }

        public void AddDateParam(DbCommand command, string paramName, DateTime value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Date, Value = value };

            command.Parameters.Add(param);
        }

        public void AddDateTimeParam(DbCommand command, string paramName, DateTime value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime, Value = value };

            command.Parameters.Add(param);
        }

        public void AddStringParam(DbCommand command, string paramName, string value)
        {
            if (value.Contains("'"))
            {
                value = value.Replace("'", "\'");
            }

            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Value = value };

            command.Parameters.Add(param);
        }

        public void AddDecimalParam(DbCommand command, string paramName, decimal value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Decimal, Value = value };

            command.Parameters.Add(param);
        }

        public void AddInt64Param(DbCommand command, string paramName, long value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.BigInt, Value = value };

            command.Parameters.Add(param);
        }

        public void AddIntParam(DbCommand command, string paramName, int value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Int, Value = value };

            command.Parameters.Add(param);
        }

        public void AddEnumParam(DbCommand command, string paramName, Enum value)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.BigInt, Value = Convert.ToInt64(value) };

            command.Parameters.Add(param);
        }

        public void AddParam(DbCommand command, string paramName, object value)
        {
            var param = new SqlParameter { ParameterName = paramName, Value = value ?? DBNull.Value };

            command.Parameters.Add(param);
        }

        public DbParameter AddIntReturnParam(DbCommand command, string paramName)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };

            command.Parameters.Add(param);

            return param;
        }

        public DbParameter AddStringReturnParam(DbCommand command, string paramName)
        {
            var param = new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Output };

            command.Parameters.Add(param);

            return param;
        }

        public DbCommand GetCommand(DbConnection connection)
        {
            return new SqlCommand("", (SqlConnection)connection);
        }

        public Compiler GetCompiler()
        {
            return new SqlServerCompiler();
        }

        public DbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
