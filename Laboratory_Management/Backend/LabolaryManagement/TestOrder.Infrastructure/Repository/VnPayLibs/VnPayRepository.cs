using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using TestOrder.Infrastructure.Models.VnPayModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Infrastructure.Repository.VnPayLibs
{
    public class VnPayRepository
    {
        private readonly SortedList<string, string> requestdata = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> responseData = new SortedList<string, string>(new VnPayCompare());

        public VnPayResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VnPayRepository();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            var orderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));

            var vnPayTrancsactionId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));

            var vnpReponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;

            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");

            var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);

            if (!checkSignature)
            {
                return new VnPayResponseModel
                {
                    IsSuccess = false,
                };
            }

            return new VnPayResponseModel
            {
                IsSuccess = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId.ToString(),
                TransactionId = vnPayTrancsactionId.ToString(),
                PaymentId = vnPayTrancsactionId.ToString(),
                Token = vnpSecureHash,
                ResponseCode = vnpReponseCode
            };
        }

        public string GetIpAddress(HttpContext context)
        {
            string ipAddress = string.Empty;

            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null)
                    {
                        ipAddress = remoteIpAddress.ToString();
                    }

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine($"Error retrieving IP address: {ex.Message}");
            }

            return "127.0.0.1";
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                requestdata.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return responseData.TryGetValue(key, out var returnValue) ? returnValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in requestdata.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var queryString = data.ToString();

            baseUrl += "?" + queryString;

            var signData = queryString;

            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);

            baseUrl += "vnp_SecureHash=" + vnpSecureHash;
            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (responseData.ContainsKey("vnp_SecureHashType"))
            {
                responseData.Remove("vnp_SecureHashType");
            }

            if (responseData.ContainsKey("vnp_SecureHash"))
            {
                responseData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }

    }
    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}

