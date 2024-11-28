using BaiGiuaky.Models.Vnpay;

namespace BaiGiuaky.Service.Vnpay
{
    public interface IVnPayService
    {
		string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
		VnPaymentResponseModel PaymentExecute(IQueryCollection collections);

	}
}
