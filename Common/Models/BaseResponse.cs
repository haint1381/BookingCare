using BookingCare.Common.Extentions;
using BookingCare.Common.SystemEnum;
using System;
using System.Collections.Generic;

namespace BookingCare.Common.Models
{
    public class BaseResponse
    {
        public BaseResponse()
        {
            Messages = new List<string>();
            ServerTime = DateTime.Now.AsUnixTimeStamp().ToString();
        }
        public bool Status { get; set; }
        public ErrorCodeEnum ErrorCode { get; set; }
        public List<string> Messages { get; set; }
        public object Data { get; set; }
        public int ShardId { get; set; }
        public int Version { get; set; }
        public string ServerTime { get; set; }

        public void SetSuccess()
        {
            Status = true;
            ErrorCode = ErrorCodeEnum.NoErrorCode;
        }
        public void SetSuccess(string message)
        {
            Status = true;
            Messages.Add(message);
            ErrorCode = ErrorCodeEnum.NoErrorCode;
        }
        public void SetFail(ErrorCodeEnum code)
        {
            Status = false;
            ErrorCode = code;
            string message = code.GetDisplayName();
            Messages.Add(message);
        }
        public void SetFail(string message, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
        {
            Status = false;
            ErrorCode = code;
            Messages.Add(message);
        }
        public void SetFail(Exception ex, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
        {
            Status = false;
            ErrorCode = code;
            string message = $"Message: {ex.Message}";
            Messages.Add(message);
        }
        public void SetFail(IEnumerable<string> messages, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
        {
            Status = false;
            ErrorCode = code;
            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }
    }
}