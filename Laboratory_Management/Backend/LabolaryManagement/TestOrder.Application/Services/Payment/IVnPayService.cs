using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.Payment;
using TestOrder.Infrastructure.Models.VnPayModels;

namespace TestOrder.Application.Services.Payment
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentRequestDTO model, HttpContext context);
        VnPayResponseModel PaymentExecute(IQueryCollection collections);
    }
}
