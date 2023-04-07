using AutoMapper;
using BookingCare.Common;
using BookingCare.Common.Extentions;
using BookingCare.Common.Models;
using BookingCare.Common.SystemEnum;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookingCare.Service
{
    public class BaseAppService : IBaseAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IMapper Mapper;


        public BaseAppService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                Mapper = _httpContextAccessor.HttpContext.RequestServices.GetService<IMapper>();
            }
        }
        protected async Task<BaseResponse> ProcessRequest(Func<BaseResponse, Task> processFunc)
        {
            BaseResponse response = new BaseResponse();
            try
            {
                await processFunc(response);
            }
            catch (Exception e)
            {
                if (e != null
                    && e.Data != null
                    && e.Data.Contains(SystemConst.ErrorCodeEnum)
                    && Enum.TryParse(e.Data[SystemConst.ErrorCodeEnum].AsString(), out ErrorCodeEnum errorCodeValue))
                {
                    response.SetFail(errorCodeValue);
                }
                else
                {
                    response.SetFail(e.Message, ErrorCodeEnum.InternalExceptions);
                    //response.SetFail(ErrorCodeEnum.InternalExceptions);
                }
            }
            return response;
        }


        protected BaseResponse ProcessRequest(Action<BaseResponse> processFunc)
        {
            BaseResponse response = new BaseResponse();

            try
            {
                processFunc(response);
            }
            catch (Exception e)
            {
                if (e.Data.Contains(SystemConst.ErrorCodeEnum) && Enum.TryParse(e.Data[SystemConst.ErrorCodeEnum].AsString(), out ErrorCodeEnum errorCodeValue))
                {
                    response.SetFail((ErrorCodeEnum)e.Data[SystemConst.ErrorCodeEnum]);
                }
                else
                {
                    response.SetFail(e.Message);
                    //response.SetFail(ErrorCodeEnum.InternalExceptions);
                }
            }

            return response;
        }
    }
}
