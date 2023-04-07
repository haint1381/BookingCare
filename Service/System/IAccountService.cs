using BookingCare.Common.Models;
using BookingCare.Common.Models.Request;
using System.Threading.Tasks;

namespace BookingCare.Service.System
{
    public interface IAccountService : IBaseAppService
    {
        Task<BaseResponse> Process(AccountGetsRequest request);
    }
}
