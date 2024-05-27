using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BaiGiuaKy.Models.MoMo
{
    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
