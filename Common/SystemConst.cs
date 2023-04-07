namespace BookingCare.Common
{
    public class SystemConst
    {
        public const string ECMConnectionString = "Server=localhost;Database=ECM;Trusted_Connection=true;MultipleActiveResultSets=true";

        public const string ErrorCodeEnum = "ErrorCode";
        public const string USER_NOT_EXIST = "User not exist";
        public static string[] SlocSales = new string[] { "3000", "7000", "7001" };
        public static string MessageSuccess = "Success";
        public static string DefaultImei = "0000-000000-000000000-00000";
        public static string FormatExcelVND = @"_(* #,##0_);_(* (#,##0);_(* ""-""??_);_(@_)";
    }
}
