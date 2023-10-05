using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace WebApplication1
{
    public class CustomRedirectMiddleware : IMiddleware
    {

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method == "POST" && context.Request.Path == "/api/Base")
            {
                var body = await GetRequestBody(context.Request);
                var updatedContext = UpdateRequest(context, body);
                await next(updatedContext);
            }
            else
            {
                await next(context);
            }
        }

        private async Task<string> GetRequestBody(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        private HttpContext UpdateRequest(HttpContext context, string body)
        {
            var json = JsonConvert.DeserializeAnonymousType(body, new
            {
                method = "",
                path = "",
                queryString = "",
                body = ""
            });

            var newRequest = context.Request;
            newRequest.Path = json.path;
            newRequest.Method = json.method;
            newRequest.QueryString = new QueryString($"?{json.queryString}");


            if (!string.IsNullOrEmpty(json.body))
            {
                byte[] bodyBytes = Encoding.UTF8.GetBytes(json.body);
                newRequest.Body = new MemoryStream(bodyBytes);
                newRequest.ContentLength = bodyBytes.Length;
            }

            return context;
        }
    }
}