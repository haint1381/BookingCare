using BookingCare.Common;
using BookingCare.Common.Models;
using BookingCare.Common.SystemEnum;
using BookingCare.Data.DBBase;
using BookingCare.DataAccess.Entity;
using Microsoft.Extensions.Logging;
using SqlKata;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingCare.DataAccess.Repositoy.System.Impl
{
    public class AccountRepository : DBBase<Account>, IAccountRepository
    {
        public AccountRepository(IDatabaseHelper databaseHelper, ILoggerFactory loggerFactory) : base(databaseHelper, loggerFactory)
        {
        }
        public override string ConnectionString => SystemConst.ECMConnectionString;

        public async Task<Account> GetByUserName(string name)
        {
            var records = new List<Account>();
            var query = new Query(TableName)
                .Where("UserName", "=", name)
                .Where("Status", AccountStatus.Active);

            await RunSqlTemplate(
                ConnectionString,
                new Query[] { query.Clone() },
                async (connection, command) =>
                {
                    using var dataReader = command.ExecuteReader();
                    while (await dataReader.ReadAsync())
                    {
                        records.Add(GetObjectFromDataReader(dataReader));
                    }
                },
                (exception, sql) => logger.LogError(exception, $"GetByUserName: {sql}")
            );

            return records.FirstOrDefault();
        }

        public async Task<IEnumerable<Account>> Search(string name, RefSqlPaging paging)
        {
            var records = new List<Account>();
            var query = new Query(TableName)
                .When(!string.IsNullOrWhiteSpace(name), q => q.WhereLike("FullName", $"%{name}%"));
            var abc = LogQuery(new[] { query });
            var queryCount = query.Clone().AsCount();
            query.OrderBy("FullName");
            query.Limit(paging.PageSize).Offset(paging.OffSet);

            await Task.WhenAll(
                RunSqlTemplate(
                    ConnectionString,
                    new Query[] { queryCount },
                    async (connection, command) =>
                    {
                        paging.TotalRow = (int)await command.ExecuteScalarAsync();
                    },
                    (exception, sql) => logger.LogError(exception, $"Search_Count: {sql}")
                ),
                RunSqlTemplate(
                    ConnectionString,
                    new Query[] { query },
                    async (connection, command) =>
                    {
                        using var dataReader = command.ExecuteReader();
                        while (await dataReader.ReadAsync())
                        {
                            records.Add(GetObjectFromDataReader(dataReader));
                        }
                    },
                    (exception, sql) => logger.LogError(exception, $"Search: {sql}")
                )
            );

            return records;
        }
    }
}
