using BookingCare.Common.Models;
using SqlKata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingCare.Data.DBBase
{
    public interface IDBBase<T>
    {
        string TableName { get; }

        #region Query
        Query SelectByFieldIn(string fieldName, Query query);
        Query SelectByFieldIn(string fieldName, IEnumerable<object> values);
        Query SelectByField(string fieldName, object value);
        Query SelectByField(IDictionary<string, object> whereFields);
        Query DeleteByField(string fieldName, object value);
        IEnumerable<Query> CreateQuery(IEnumerable<T> newRecords);
        IEnumerable<Query> CreateQuery(T newRecord);
        IEnumerable<Query> UpdateQuery(T record);
        IEnumerable<Query> UpdateQuery(IEnumerable<T> records);
        IEnumerable<Query> UpdateQuery(T record, IEnumerable<string> ignoreFields);
        IEnumerable<Query> UpdateQuery(IEnumerable<T> records, IEnumerable<string> ignoreFields);
        IEnumerable<Query> DeleteQuery(object id);
        IEnumerable<Query> DeleteQueries<K>(IEnumerable<K> ids);
        IEnumerable<Query> DeleteManyQueries(IDictionary<string, object> whereFields);
        Query UpdateFieldsByPrimaryKeyQuery(string primaryValue, IDictionary<string, object> updateFields);
        #endregion

        Task<bool> Create(T newRecord);
        Task<bool> Create(IEnumerable<T> newRecords);
        Task<bool> Delete(object id);
        Task<bool> Update(T record);
        Task<bool> Update(IEnumerable<T> records);
        Task<bool> UpdateFieldsByPrimaryKey(object primaryKeyValue, IDictionary<string, object> updateFields);
        Task<bool> IsRecordExist(object ID);
        Task<T> GetRecordByID(object id, bool usingReadDB = false);
        Task<IEnumerable<T>> GetRecordByIDs<K>(IEnumerable<K> ids, bool usingReadDB = false);
        Task<long> Count();
        Task<IEnumerable<T>> GetAllRecords(bool usingReadDB = false);
        Task<IEnumerable<T>> GetAllRecords(RefSqlPaging paging);
    }
}
