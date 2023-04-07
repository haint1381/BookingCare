using AutoMapper;
using BookingCare.Common.Extentions;
using BookingCare.Common.Models;
using BookingCare.Common.Models.Request;
using BookingCare.Common.Models.Response;
using BookingCare.DataAccess.Repositoy.System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookingCare.Service.System.Impl
{
    public class AccountService : BaseAppService, IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(
            IAccountRepository accountRepository,
            IHttpContextAccessor httpContextAccessor
        ) : base(httpContextAccessor)
        {
            _accountRepository = accountRepository;
        }

        public async Task<BaseResponse> Process(AccountGetsRequest request)
        {
            return await ProcessRequest(async (response) =>
            {
                var paging = new RefSqlPaging(request.PageIndex, request.PageSize);
                var result = await _accountRepository.Search(
                    keyword: request.KeyWord, paging
                );

                response.Data = new
                {
                    model = result.IsNullOrEmpty() ? Array.Empty<AccountModel>() : Mapper.Map<AccountModel[]>(result),
                    paging.PageIndex,
                    paging.PageSize,
                    paging.TotalRow
                };
                response.SetSuccess();
            });
        }
    }
}
