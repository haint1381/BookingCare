using BookingCare.Common.Models;
using BookingCare.Data.DBBase;
using BookingCare.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingCare.DataAccess.Repositoy.System
{
    public interface IAccountRepository : IDBBase<Account>
    {
        Task<IEnumerable<Account>> Search(
            string keyword,
            RefSqlPaging paging
            );
    }
}
