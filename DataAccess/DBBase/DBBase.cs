using BookingCare.Common.Extentions;
using BookingCare.Common.Models;
using Microsoft.Extensions.Logging;
using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BookingCare.Data.DBBase
{
    public abstract class DBBase<T> : IDBBase<T>, IDisposable
    {
        protected readonly DateTime MinValueUtc = new DateTime(0L, DateTimeKind.Utc);
        protected readonly Type typeOfT;
        protected readonly IDatabaseHelper databaseHelper;
        protected readonly ILoggerFactory loggerFactory;
        protected ILogger logger;
        protected readonly Compiler compiler;
        private readonly int MAX_UPDATE_QUERIES = 50;

        public DBBase(
            IDatabaseHelper databaseHelper,
            ILoggerFactory loggerFactory
        )
        {
            this.databaseHelper = databaseHelper;
            this.loggerFactory = loggerFactory;

            logger = loggerFactory.CreateLogger<DBBase<T>>();
            compiler = databaseHelper.GetCompiler();
            typeOfT = typeof(T);
            TableName = typeOfT.Name;
        }

        #region Table Specific
        public virtual string TableName { get; protected set; }
        public virtual string ConnectionString { get; set; }
        public virtual string ReadConnectionString { get; set; }

        protected virtual string PrimaryKey
        {
            get
            {
                return "Id";
            }
        }

        protected virtual bool IsAutoIncreasementPrimaryKey => false;
        #endregion

        #region IDisposable
        public virtual void Dispose()
        {

        }
        #endregion

        #region Query Sql
        public Query SelectByFieldIn(string fieldName, Query query)
        {
            return new Query(TableName).WhereIn(fieldName, query);
        }

        public Query SelectByFieldIn(string fieldName, IEnumerable<object> values)
        {
            return new Query(TableName).WhereIn(fieldName, values);
        }

        public Query SelectByField(string fieldName, object value)
        {
            return new Query(TableName).Where(fieldName, value);
        }

        public Query SelectByField(IDictionary<string, object> whereFields)
        {
            return new Query(TableName).Where(new ReadOnlyDictionary<string, object>(whereFields));
        }

        public Query DeleteByField(string fieldName, object value)
        {
            return new Query(TableName).AsDelete().Where(fieldName, value);
        }

        public virtual IEnumerable<Query> CreateQuery(IEnumerable<T> newRecords)
        {
            var queries = new List<Query>();

            if (!newRecords.IsNullOrEmpty())
            {
                var insertFields = IsAutoIncreasementPrimaryKey
                    ? DBFields.Where(f => f.Name != PrimaryKey).OrderBy(field => field.Name)
                    : DBFields.OrderBy(field => field.Name);
                var columns = insertFields.Select(field => field.Name);
                var data = newRecords.Select(record => insertFields.Select(field => field.GetValue(record)));

                queries.Add(new Query(TableName).AsInsert(columns, data));
            }

            return queries;
        }

        public virtual IEnumerable<Query> CreateQuery(IEnumerable<T> newRecords, IEnumerable<string> ignoreFields)
        {
            var queries = new List<Query>();

            if (!newRecords.IsNullOrEmpty())
            {
                var dbFilterFields = ignoreFields.IsNullOrEmpty() ? DBFields : DBFields.Where(i => !ignoreFields.Contains(i.Name));
                var insertFields = IsAutoIncreasementPrimaryKey
                    ? dbFilterFields.Where(f => f.Name != PrimaryKey).OrderBy(field => field.Name)
                    : dbFilterFields.OrderBy(field => field.Name);
                var columns = insertFields.Select(field => field.Name);
                var data = newRecords.Select(record => insertFields.Select(field => field.GetValue(record)));

                queries.Add(new Query(TableName).AsInsert(columns, data));
            }

            return queries;
        }

        public virtual IEnumerable<Query> CreateQuery(T newRecord)
        {
            var insertFields = new Dictionary<string, object>();
            var dbFields = IsAutoIncreasementPrimaryKey
                ? DBFields.Where(f => f.Name != PrimaryKey).OrderBy(field => field.Name)
                : DBFields.OrderBy(field => field.Name);

            dbFields.Loop((index, field) => insertFields.Add(field.Name, field.GetValue(newRecord)));

            var query = new Query(TableName).AsInsert(insertFields);

            return new Query[] { query };
        }

        public virtual IEnumerable<Query> UpdateQuery(T record)
        {
            var primaryKeyValue = DBFields
                .First(field => field.Name.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                .GetValue(record);
            var ignoreFields = new[] { "createddate", "createddateutc", "createduid", PrimaryKey.ToLower() };
            var updateFields = new Dictionary<string, object>();
            DBFields
                .Where(field => !ignoreFields.Contains(field.Name.ToLower()))
                .Loop((index, field) => updateFields.Add(field.Name, field.GetValue(record)));
            var query = new Query(TableName).AsUpdate(updateFields).Where(PrimaryKey, primaryKeyValue);

            return new Query[] { query };
        }

        public virtual IEnumerable<Query> UpdateQuery(IEnumerable<T> records)
        {
            var queries = new List<Query>();

            foreach (var record in records)
            {
                var primaryKeyValue = DBFields
                    .First(field => field.Name.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                    .GetValue(record);
                var updateFields = new Dictionary<string, object>();
                DBFields
                    .Where(field => !field.Name.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                    .Loop((index, field) => updateFields.Add(field.Name, field.GetValue(record)));
                var query = new Query(TableName).AsUpdate(updateFields).Where(PrimaryKey, primaryKeyValue);

                queries.Add(query);
            }

            return queries.ToArray();
        }

        public virtual IEnumerable<Query> UpdateQuery(T record, IEnumerable<string> ignoreFields)
        {
            var primaryKeyValue = DBFields
                .First(field => field.Name.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                .GetValue(record);
            var updateFields = new Dictionary<string, object>();
            var dbFilterFields = ignoreFields.IsNullOrEmpty() ? DBFields : DBFields.Where(i => !ignoreFields.Contains(i.Name));

            dbFilterFields
                .Where(field => !field.Name.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                .Loop((index, field) => updateFields.Add(field.Name, field.GetValue(record)));
            var query = new Query(TableName).AsUpdate(updateFields).Where(PrimaryKey, primaryKeyValue);

            return new Query[] { query };
        }

        public virtual IEnumerable<Query> UpdateQuery(IEnumerable<T> records, IEnumerable<string> ignoreFields)
        {
            var queries = new List<Query>();
            foreach (var record in records)
            {
                var primaryKeyValue = DBFields
                    .First(field => field.Name.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                    .GetValue(record);
                var updateFields = new Dictionary<string, object>();
                var dbFilterFields = ignoreFields.IsNullOrEmpty() ? DBFields : DBFields.Where(i => !ignoreFields.Contains(i.Name));

                dbFilterFields
                    .Where(field => !field.Name.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                    .Loop((index, field) => updateFields.Add(field.Name, field.GetValue(record)));
                var query = new Query(TableName).AsUpdate(updateFields).Where(PrimaryKey, primaryKeyValue);
                queries.Add(query);
            }

            return queries.ToArray();
        }

        public virtual IEnumerable<Query> DeleteQuery(object id)
        {
            var query = new Query(TableName).AsDelete().Where(PrimaryKey, id);

            return new Query[] { query };
        }

        public virtual IEnumerable<Query> DeleteQueries<K>(IEnumerable<K> ids)
        {
            var query = new Query(TableName).AsDelete().WhereIn(PrimaryKey, ids);

            return new Query[] { query };
        }

        public virtual IEnumerable<Query> DeleteManyQueries(IDictionary<string, object> whereFields)
        {
            var query = new Query(TableName).AsDelete().Where(new ReadOnlyDictionary<string, object>(whereFields));

            return new Query[] { query };
        }
        #endregion

        #region Parameter Common Functions
        protected void AddParam(DbCommand command, Dictionary<string, object> values)
        {
            foreach (var value in values)
            {
                AddParam(command, value.Key, value.Value);
            }
        }

        protected void AddParam(DbCommand command, string paramName, object value)
        {
            if (value is string stringValue)
            {
                databaseHelper.AddStringParam(command, paramName, stringValue);
            }
            else if (value is long longValue)
            {
                databaseHelper.AddInt64Param(command, paramName, longValue);
            }
            else if (value is int intValue)
            {
                databaseHelper.AddIntParam(command, paramName, intValue);
            }
            else if (value is decimal decimalValue)
            {
                databaseHelper.AddDecimalParam(command, paramName, decimalValue);
            }
            else if (value is DateTime dateTimeValue)
            {
                //if (dateTimeValue == dateTimeValue.Date)
                //{
                //    databaseHelper.AddDateParam(command, paramName, dateTimeValue);
                //}
                //else
                //{
                databaseHelper.AddDateTimeParam(command, paramName, dateTimeValue);
                //}
            }
            else if (value is Guid guidValue)
            {
                databaseHelper.AddGuidParam(command, paramName, guidValue);
            }
            else if (value is Enum enumValue)
            {
                databaseHelper.AddEnumParam(command, paramName, enumValue);
            }
            else if (value is byte[] bytes)
            {
                databaseHelper.AddByteArrayParam(command, paramName, bytes);
            }
            else
            {
                databaseHelper.AddParam(command, paramName, value);
            }
        }
        #endregion

        #region General Functions
        protected void CheckConnection(DbConnection connection)
        {
            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        protected object GetValueFromDataReader(DbDataReader dr, int i)
        {
            var fieldType = dr.GetFieldType(i);
            object result = null;

            try
            {
                if (dr[i] == DBNull.Value)
                {
                    result = null;
                }
                else if (fieldType == typeof(Guid))
                {
                    result = dr.GetGuid(i);
                }
                else if (fieldType == typeof(DateTime))
                {
                    result = dr.GetDateTime(i);
                }
                else if (fieldType == typeof(int))
                {
                    result = dr.GetInt32(i);
                }
                else if (fieldType == typeof(long))
                {
                    result = dr.GetInt64(i);
                }
                else if (fieldType == typeof(decimal))
                {
                    result = dr.GetDecimal(i);
                }
                else if (fieldType == typeof(string))
                {
                    result = dr.GetString(i);
                }
                else if (fieldType == typeof(double))
                {
                    result = dr.GetDouble(i);
                }
                else if (fieldType == typeof(bool))
                {
                    result = dr.GetBoolean(i);
                }
                else if (fieldType == typeof(byte[]))
                {
                    result = dr.GetFieldValue<byte[]>(i);
                }
                else
                {
                    result = dr.GetValue(i);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    logger.LogError(ex, $"GetValueFromDataReader: FieldName[{dr.GetName(i)}] FieldType[{fieldType.Name}] FieldValue[{dr[i]}]");
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"GetValueFromDataReader: FieldName[{dr.GetName(i)}] FieldType[{fieldType.Name}]");
                }

                throw ex;
            }

            return result;
        }

        protected object GetObjectFromDataReader(Type fieldType, DbDataReader dr, int i)
        {
            object result = null;

            var dbValue = GetValueFromDataReader(dr, i);

            try
            {
                if (fieldType == typeof(Guid))
                {
                    if (dbValue != null)
                    {
                        if (dbValue is Guid)
                        {
                            result = dbValue;
                        }
                        else if (Guid.TryParse(dbValue.ToString(), out Guid guid))
                        {
                            result = guid;
                        }
                    }
                    else
                    {
                        result = Guid.Empty;
                    }
                }
                else if (fieldType.IsEnum)
                {
                    if (dbValue != null && Enum.TryParse(fieldType, dbValue.ToString(), true, out object enumObject))
                    {
                        result = enumObject;
                    }
                }
                else if (fieldType == typeof(DateTime))
                {
                    result = dbValue ?? MinValueUtc;
                }
                else if (fieldType == typeof(int))
                {
                    result = dbValue == null ? int.MinValue : Convert.ToInt32(dbValue);
                }
                else if (fieldType == typeof(long))
                {
                    result = dbValue == null ? long.MinValue : Convert.ToInt64(dbValue);
                }
                else if (fieldType == typeof(decimal))
                {
                    result = dbValue == null ? decimal.MinValue : Convert.ToDecimal(dbValue);
                }
                else if (fieldType == typeof(double))
                {
                    result = dbValue == null ? double.MinValue : Convert.ToDouble(dbValue);
                }
                else if (fieldType == typeof(string))
                {
                    result = dbValue == null ? "" : Convert.ToString(dbValue);
                }
                else if (fieldType == typeof(bool))
                {
                    result = dbValue != null && Convert.ToBoolean(dbValue);
                }
                else
                {
                    result = dbValue;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    logger.LogError(ex, $"GetObjectFromDataReader: FieldName[{dr.GetName(i)}] FieldType[{fieldType.Name}] FieldValue[{dbValue}]");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "GetObjectFromDataReader");
                }
            }

            return result;
        }
        #endregion

        #region Template Functions
        protected async Task<bool> CallStoreProcedure(
            string connectionString,
            string storeProcedureName,
            Func<DbCommand, Task> executeSP,
            Action<Exception> whenException
        )
        {
            var stopWatch = new Stopwatch();
            var result = true;

            using var connection = databaseHelper.GetConnection(connectionString);
            try
            {
                using var command = databaseHelper.GetCommand(connection);
                command.CommandText = storeProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                CheckConnection(connection);

                stopWatch.Start();
                await executeSP(command);
                stopWatch.Stop();

            }
            catch (Exception ex)
            {
                if (whenException != null)
                {
                    whenException?.Invoke(ex);
                }
                else
                {
                    logger.LogError(ex, $"CallStoreProcedure[{storeProcedureName}]");
                }

                result = false;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        //protected async Task<bool> CallStoreProcedureWithConnection(
        //    string storeProcedureName,
        //    DbConnection connection,
        //    Func<DbCommand, Task> executeSP,
        //    Action<Exception> whenException
        //)
        //{
        //    var stopWatch = new Stopwatch();
        //    var result = true;

        //    try
        //    {
        //        using var command = databaseHelper.GetCommand(connection);
        //        command.CommandText = storeProcedureName;
        //        command.CommandType = CommandType.StoredProcedure;

        //        CheckConnection(connection);

        //        stopWatch.Start();
        //        await executeSP(command);
        //        stopWatch.Stop();

        //        _ = connection.CloseAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (whenException != null)
        //        {
        //            whenException?.Invoke(ex);
        //        }
        //        else
        //        {
        //            logger.LogError(ex, $"CallStoreProcedureWithConnection[{storeProcedureName}]");
        //        }

        //        result = false;
        //    }

        //    return result;
        //}

        protected virtual async Task<bool> RunRawSqlWithTransaction(
            string connectionString,
            string sql,
            Action<Exception, string> whenException
        )
        {
            return await RunSqlTemplate(
                connectionString,
                sql,
                async (connection, command) =>
                {
                    command.CommandText = databaseHelper.TransactionIsolationLevel
                        + databaseHelper.BeginTry
                        + databaseHelper.BeginTransaction
                        + command.CommandText
                        + databaseHelper.CommitTransaction
                        + databaseHelper.EndTryBeginCatch
                        + databaseHelper.RaisError
                        + databaseHelper.EndCatch
                    ;

                    await command.ExecuteNonQueryAsync();
                },
                (exception, sql) =>
                {
                    whenException?.Invoke(exception, sql);
                }
            );
        }

        protected virtual async Task<bool> RunSqlTemplateWithTransaction(
            string connectionString,
            IEnumerable<Query> queries,
            Action<Exception, string> whenException
        )
        {
            return await RunSqlTemplate(
                connectionString,
                queries,
                async (connection, command) =>
                {
                    command.CommandText = databaseHelper.TransactionIsolationLevel
                        + databaseHelper.BeginTry
                        + databaseHelper.BeginTransaction
                        + command.CommandText
                        + databaseHelper.CommitTransaction
                        + databaseHelper.EndTryBeginCatch
                        + databaseHelper.RaisError
                        + databaseHelper.RollBackTransaction
                        + databaseHelper.EndCatch
                    ;

                    await command.ExecuteNonQueryAsync();
                },
                (exception, sql) =>
                {
                    whenException?.Invoke(exception, sql);
                }
            );
        }


        protected virtual async Task<bool> RunSqlTemplateWithNoTransaction(
            string connectionString,
            IEnumerable<Query> queries,
            Action<Exception, string> whenException
        )
        {
            return await RunSqlTemplate(
                connectionString,
                queries,
                async (connection, command) =>
                {
                    await command.ExecuteNonQueryAsync();
                },
                (exception, sql) =>
                {
                    whenException?.Invoke(exception, sql);
                }
            );
        }

        protected async Task<bool> RunSqlTemplate(
            string connectionString,
            string sql,
            Func<DbConnection, DbCommand, Task> runSqlMethod,
            Action<Exception, string> whenException
        )
        {
            var stopWatch = new Stopwatch();
            var result = true;

            using var connection = databaseHelper.GetConnection(connectionString);
            try
            {
                using var command = databaseHelper.GetCommand(connection);

                stopWatch.Start();

                command.CommandText = sql;

                CheckConnection(connection);

                await runSqlMethod(connection, command);
                stopWatch.Stop();

            }
            catch (Exception ex)
            {
                if (whenException != null)
                {
                    whenException?.Invoke(ex, sql);
                }
                else
                {
                    logger.LogError(ex, $"RunSqlTemplate from  with sql: {sql}");
                }

                result = false;
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        protected async Task<bool> RunSqlTemplate(
            string connectionString,
            IEnumerable<Query> queries,
            Func<DbConnection, DbCommand, Task> runSqlMethod,
            Action<Exception, string> whenException,
            bool isSetGroupConcatMaxLen = false
        )
        {
            var stopWatch = new Stopwatch();
            var result = true;
            var sql = string.Empty;

            using var connection = databaseHelper.GetConnection(connectionString);
            try
            {
                using var command = databaseHelper.GetCommand(connection);

                stopWatch.Start();
                var compileResult = compiler.Compile(queries);
                var group_concat = isSetGroupConcatMaxLen ? databaseHelper.GroupByConcatMaxLen : "";
                command.CommandText += $"{group_concat}{compileResult.Sql}";
                sql = command.CommandText;

                if (!compileResult.NamedBindings.IsNullOrEmpty())
                {
                    AddParam(command, compileResult.NamedBindings);
                }

                CheckConnection(connection);

                await runSqlMethod(connection, command);
                stopWatch.Stop();

            }
            catch (Exception ex)
            {
                if (whenException != null)
                {
                    whenException?.Invoke(ex, sql);
                }
                else
                {
                    logger.LogError(ex, $"RunSqlTemplate from  with sql: {sql}");
                }
                result = false;
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return result;
        }



        protected async Task<bool> RunSqlTemplateWithConnection(
            IEnumerable<Query> queries,
            DbConnection connection,
            Func<DbCommand, Task> runSqlMethod,
            Action<Exception, string> whenException
        )
        {
            var stopWatch = new Stopwatch();
            var result = true;
            var sql = "";

            using var command = databaseHelper.GetCommand(connection);
            try
            {

                stopWatch.Start();
                var compileResult = compiler.Compile(queries);

                command.CommandText += compileResult.Sql;

                if (!compileResult.NamedBindings.IsNullOrEmpty())
                {
                    AddParam(command, compileResult.NamedBindings);
                }

                CheckConnection(connection);

                sql = command.CommandText;

                await runSqlMethod(command);
                stopWatch.Stop();

            }
            catch (Exception ex)
            {
                if (whenException != null)
                {
                    whenException?.Invoke(ex, sql);
                }
                else
                {
                    logger.LogError(ex, $"RunSqlTemplateWithConnection from  with sql: {sql}");
                }

                result = false;
                throw ex;
            }
            finally
            {
                connection.Close();

            }

            return result;
        }
        #endregion

        #region DB Functions
        private IEnumerable<PropertyInfo> dbFields;
        protected IEnumerable<PropertyInfo> DBFields
        {
            get
            {
                if (dbFields.IsNullOrEmpty())
                {
                    //exclude related data fields [for now related data fields is IList interface]
                    dbFields = typeOfT.GetProperties()
                        .Where(p =>
                            !(p.PropertyType.IsGenericType && p.PropertyType.IsInterface)
                            && (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
                            && p.GetSetMethod() != null && p.GetSetMethod().IsPublic
                            && !p.Name.Equals("NumericalOrder")
                        )
                        .Select(p => p);
                }

                return dbFields;
            }
        }

        public virtual object GetIDFromRecord(T record)
        {
            var field = typeOfT.GetField(PrimaryKey);
            var result = default(object);

            if (field != null)
            {
                result = field.GetValue(record);
            }

            return result;
        }

        protected T GetObjectFromDataReader(DbDataReader dr)
        {
            if (dr.HasRows)
            {
                return ConvertFromDataReaderToObject<T>(dr);
            }

            return default;
        }

        protected K ConvertFromDataReaderToObject<K>(DbDataReader dr)
        {
            K record = Activator.CreateInstance<K>();

            for (int i = 0; i < dr.FieldCount; i++)
            {
                var property = typeof(K).GetProperty(dr.GetName(i));

                if (property != null)
                {
                    var value = GetObjectFromDataReader(property.PropertyType, dr, i);

                    try
                    {
                        if (value != null)
                        {
                            var propertyType = property.PropertyType;
                            var isNullable = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>));

                            if (isNullable)
                            {
                                var targetType = Nullable.GetUnderlyingType(propertyType);
                                if (targetType.IsEnum)
                                {
                                    object enumValue = Enum.ToObject(Nullable.GetUnderlyingType(property.PropertyType), value);
                                    property.SetValue(record, enumValue, null);
                                }
                                else
                                {
                                    property.SetValue(record, Convert.ChangeType(value, targetType));
                                }
                            }
                            else
                            {
                                property.SetValue(record, propertyType.IsEnum ? Enum.ToObject(propertyType, value) : value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"DB{typeOfT.Name}.{property.Name}.ConvertFromDataReaderToObject[{value}]");
                    }
                }
            }

            return record;
        }

        public virtual async Task<long> Count()
        {
            long count = 0;

            await RunSqlTemplate(
                ConnectionString,
                new Query[] { new Query(TableName).AsCount() },
                async (connection, command) =>
                {
                    var result = await command.ExecuteScalarAsync();

                    count = Convert.ToInt64(result);
                },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Count[{typeOfT.Name}]: {sql}");
                }
            );

            return count;
        }

        public virtual async Task<IEnumerable<T>> GetAllRecords(bool usingReadDB = false)
        {
            var connectionString = usingReadDB ? ReadConnectionString : ConnectionString;
            var result = new List<T>();

            await RunSqlTemplate(
                connectionString,
                new Query[] { new Query(TableName) },
                async (connection, command) =>
                {
                    using var dataReader = command.ExecuteReader();
                    while (await dataReader.ReadAsync())
                    {
                        result.Add(GetObjectFromDataReader(dataReader));
                    }
                },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"GetAllRecords[{typeOfT.Name}]: {sql}");
                }
            );

            return result;
        }

        public virtual async Task<IEnumerable<T>> GetAllRecords(int pageIndex, int pageSize)
        {
            var result = new List<T>();
            await RunSqlTemplate(
                ConnectionString,
                new Query[] { new Query(TableName).ForPage(pageIndex + 1, pageSize) },
                async (connection, command) =>
                {
                    using var dataReader = command.ExecuteReader();
                    while (await dataReader.ReadAsync())
                    {
                        result.Add(GetObjectFromDataReader(dataReader));
                    }
                },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"GetAllRecords[{typeOfT.Name}]: {sql}");
                }
            );

            return result;
        }

        public async Task<IEnumerable<T>> GetAllRecords(RefSqlPaging paging)
        {
            var records = new List<T>();
            var query = new Query(TableName);

            await Task.WhenAll(
                RunSqlTemplate(
                    ConnectionString,
                    new Query[] { query.Clone().AsCount() },
                    async (connection, command) =>
                    {
                        paging.TotalRow = (int)await command.ExecuteScalarAsync();
                    },
                    (exception, sql) => logger.LogError(exception, $"GetAllRecords_Count: {sql}")
                ),
                RunSqlTemplate(
                    ConnectionString,
                    new Query[] { query.Clone().ForPage(paging.PageIndex + 1, paging.PageSize) },
                    async (connection, command) =>
                    {
                        using var dataReader = command.ExecuteReader();
                        while (await dataReader.ReadAsync())
                        {
                            records.Add(GetObjectFromDataReader(dataReader));
                        }
                    },
                    (exception, sql) => logger.LogError(exception, $"GetAllRecords: {sql}")
                )
            );

            return records;
        }

        public virtual async Task<T> GetRecordByID(object id, bool usingReadDB = false)
        {
            var result = default(T);
            var query = new Query(TableName).Where(PrimaryKey, id);
            var connectionString = usingReadDB ? ReadConnectionString : ConnectionString;

            await RunSqlTemplate(
                connectionString,
                new Query[] { query },
                async (connection, command) =>
                {
                    using var dataReader = command.ExecuteReader();
                    while (await dataReader.ReadAsync())
                    {
                        result = GetObjectFromDataReader(dataReader);
                    }
                },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"GetRecordByID[{typeOfT.Name}]: {sql}");
                }
            );

            return result;
        }

        public virtual async Task<IEnumerable<T>> GetRecordByIDs<K>(IEnumerable<K> ids, bool usingReadDB = false)
        {
            var items = new List<T>();

            if (ids.IsNullOrEmpty())
            {
                return items;
            }

            var query = new Query(TableName);

            if (ids.Count() > 2000)
            {
                var strIds = string.Join(";", ids.Distinct());

                query.WithRaw("TBL", $"SELECT [data] as tbl_ID from dbo.nop_splitstring_to_table('{strIds}', ';')")
                    .WhereRaw($"Exists(SELECT 1 tbl_ID FROM TBL WHERE tbl_ID = {PrimaryKey})");
            }
            else
            {
                query.WhereIn(PrimaryKey, ids);
            }

            var res = await RunSqlTemplate(
                ConnectionString,
                new Query[] { query },
                async (connection, command) =>
                {
                    using var dataReader = await command.ExecuteReaderAsync();

                    while (await dataReader.ReadAsync())
                    {
                        items.Add(ConvertFromDataReaderToObject<T>(dataReader));
                    }
                },
                (exception, sql) =>
                {
                    logger.LogError($"GetRecordByIDs[{typeOfT.Name}] - {(ids == null ? "null" : string.Join(",", ids))}");
                }
            );
            return items;
        }

        public virtual async Task<bool> Create(T newRecord)
        {
            return await RunSqlTemplateWithNoTransaction(
                ConnectionString,
                CreateQuery(newRecord),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Create[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Create(T newRecord, Action<Exception> handleException)
        {
            return await RunSqlTemplateWithNoTransaction(
                ConnectionString,
                CreateQuery(newRecord),
                (exception, sql) =>
                {
                    handleException(exception);
                    logger.LogError(exception, $"Create[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Create(IEnumerable<T> newRecords)
        {
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                CreateQuery(newRecords),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Create[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Create(IEnumerable<T> newRecords, IEnumerable<string> ignoreFields)
        {
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                CreateQuery(newRecords, ignoreFields),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Create[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Delete(object id)
        {
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                DeleteQuery(id),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Delete[{typeOfT.Name}]: {sql}");
                }
            );
        }

        public virtual async Task<bool> DeleteMany<K>(IEnumerable<K> ids)
        {
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                DeleteQueries(ids),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Delete[{typeOfT.Name}]: {sql}");
                }
            );
        }

        public virtual async Task<bool> UpdateFieldsByPrimaryKey(object primaryKeyValue, IDictionary<string, object> updateFields)
        {
            var updateQuery = new Query(TableName)
                .Where(PrimaryKey, primaryKeyValue)
                .AsUpdate(new ReadOnlyDictionary<string, object>(updateFields));
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                new Query[] { updateQuery },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"UpdateFieldsByPrimaryKey[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Update(T record)
        {
            return await RunSqlTemplateWithNoTransaction(
                ConnectionString,
                UpdateQuery(record),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Update[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Update(IEnumerable<T> records)
        {
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                UpdateQuery(records),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Update[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Update(T record, IEnumerable<string> ignoreFields)
        {
            return await RunSqlTemplateWithNoTransaction(
                ConnectionString,
                UpdateQuery(record, ignoreFields),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Update[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> Update(IEnumerable<T> records, IEnumerable<string> ignoreFields)
        {
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                UpdateQuery(records, ignoreFields),
                (exception, sql) =>
                {
                    logger.LogError(exception, $"Update[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> UpdateFieldsByCondition(IDictionary<string, object> whereFields, IDictionary<string, object> updateFields)
        {
            var updateQuery = new Query(TableName)
                .Where(new ReadOnlyDictionary<string, object>(whereFields))
                .AsUpdate(new ReadOnlyDictionary<string, object>(updateFields));
            return await RunSqlTemplateWithTransaction(
                ConnectionString,
                new Query[] { updateQuery },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"UpdateFieldsByCondition[{typeOfT.Name}]: {sql}");
                    throw exception;
                }
            );
        }

        public virtual async Task<bool> IsRecordExist(object ID)
        {
            var result = false;
            var query = new Query(TableName).SelectRaw("COUNT(*)").Where(PrimaryKey, ID);
            var compileResult = compiler.Compile(query);

            await RunSqlTemplate(
                ConnectionString,
                new Query[] { query },
                async (connection, command) =>
                {
                    var count = await command.ExecuteScalarAsync();

                    result = (int)count > 0;
                },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"IsRecordExist[{typeOfT.Name}]: {sql}");
                }
            );

            return result;
        }
        #endregion

        protected bool IsTableNotExist(Exception exception)
        {
            foreach (var value in exception.Data.Values)
            {
                int.TryParse(value.ToString(), out var errorCode);

                if (errorCode == 1146) return true;
            }

            return false;
        }

        public string LogQuery(IEnumerable<Query> queries)
        {
            try
            {
                var compileResult = compiler.Compile(queries);
                return compileResult.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Query UpdateFieldsByPrimaryKeyQuery(string primaryValue, IDictionary<string, object> updateFields)
        {
            var updateQuery = new Query(TableName)
               .Where(PrimaryKey, primaryValue)
               .AsUpdate(new ReadOnlyDictionary<string, object>(updateFields));

            return updateQuery;
        }

        public async Task<T> GetRecordIgnoreMappingByID(object id, bool usingReadDB = false)
        {
            var result = default(T);
            var query = new Query(TableName).Where(PrimaryKey, id);
            var connectionString = usingReadDB ? ReadConnectionString : ConnectionString;

            await RunSqlTemplate(
                connectionString,
                new Query[] { query },
                async (connection, command) =>
                {
                    using var dataReader = command.ExecuteReader();
                    while (await dataReader.ReadAsync())
                    {
                        result = GetObjectFromDataReader(dataReader);
                    }
                },
                (exception, sql) =>
                {
                    logger.LogError(exception, $"GetRecordIgnoreMappingByID[{typeOfT.Name}]: {sql}");
                }
            );

            return result;
        }

        public virtual async Task<IEnumerable<T>> GetRecordIgnoreMappingByIDs<K>(IEnumerable<K> ids, bool usingReadDB = false)
        {
            var items = new List<T>();

            if (ids.IsNullOrEmpty())
            {
                return items;
            }

            var query = new Query(TableName);

            if (ids.Count() > 2000)
            {
                var strIds = string.Join(";", ids.Distinct());

                query.WithRaw("TBL", $"SELECT [data] as tbl_ID from dbo.nop_splitstring_to_table('{strIds}', ';')")
                    .WhereRaw($"Exists(SELECT 1 tbl_ID FROM TBL WHERE tbl_ID = {PrimaryKey})");
            }
            else
            {
                query.WhereIn(PrimaryKey, ids);
            }

            var res = await RunSqlTemplate(
                ConnectionString,
                new Query[] { query },
                async (connection, command) =>
                {
                    using var dataReader = await command.ExecuteReaderAsync();

                    while (await dataReader.ReadAsync())
                    {
                        items.Add(ConvertFromDataReaderToObject<T>(dataReader));
                    }
                },
                (exception, sql) =>
                {
                    logger.LogError($"GetRecordIgnoreMappingByIDs[{typeOfT.Name}] - {(ids == null ? "null" : string.Join(",", ids))}");
                }
            );
            return items;
        }
    }
}
