using BookingCare.Common.SystemEnum;
using System;

namespace BookingCare.DataAccess.Entity
{
    public class Account
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public AccountStatus Status { get; set; }
    }
}
