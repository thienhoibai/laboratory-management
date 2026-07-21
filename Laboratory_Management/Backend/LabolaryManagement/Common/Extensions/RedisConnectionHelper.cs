using System;

namespace Common.Extensions
{
    public static class RedisConnectionHelper
    {
        public static string ConvertConnectionString(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return "localhost:6379,abortConnect=false";
            }

            // If it doesn't start with redis:// or rediss://, assume it's already in the StackExchange format
            if (!rawUrl.StartsWith("redis://") && !rawUrl.StartsWith("rediss://"))
            {
                if (!rawUrl.Contains("abortConnect="))
                {
                    return rawUrl.Trim().TrimEnd(',') + ",abortConnect=false";
                }
                return rawUrl;
            }

            try
            {
                var uri = new Uri(rawUrl);
                var host = uri.Host;
                var port = uri.Port == -1 ? 6379 : uri.Port;
                var userInfo = uri.UserInfo;
                var password = "";

                if (!string.IsNullOrEmpty(userInfo))
                {
                    var parts = userInfo.Split(':');
                    password = parts.Length > 1 ? parts[1] : parts[0];
                }

                var isSsl = rawUrl.StartsWith("rediss://");
                var connStr = $"{host}:{port}";

                if (!string.IsNullOrEmpty(password))
                {
                    connStr += $",password={password}";
                }

                if (isSsl)
                {
                    connStr += ",ssl=true,abortConnect=false";
                }
                else
                {
                    connStr += ",abortConnect=false";
                }

                return connStr;
            }
            catch
            {
                return rawUrl;
            }
        }
    }
}
