namespace BookingCare.Common.Models
{
    public class RefSqlPaging
    {
        public RefSqlPaging(int pageIndex) : this(pageIndex, 30)
        {
        }

        public RefSqlPaging(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;

            if (PageSize <= 0)
            {
                PageSize = 30;
            }

            if (PageIndex <= 0)
            {
                PageIndex = 0;
            }
        }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int OffSet => PageIndex * PageSize;
        public long TotalRow { get; set; }
    }
}
