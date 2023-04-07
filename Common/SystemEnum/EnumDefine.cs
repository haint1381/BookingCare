using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BookingCare.Common.SystemEnum
{
    public enum ErrorCodeEnum
    {
        NoErrorCode = 0,
        Success = 1,
        Fail = 2,
        InternalExceptions = 500,
    }

    public enum AccountStatus
    {
        [Display(Name = "Hoạt động")]
        Active = 1,
        [Display(Name = "Đã khóa")]
        InActive = 2
    }
}
