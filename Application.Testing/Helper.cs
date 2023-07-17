using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using Application.DTOs;
using Application.DTOs.User;
using Application.Persistence.Repositories;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Testing
{
    public static class Helper
    {
        static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            //AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        public static StringContent ToStringContent(dynamic o)
        {
            var json = JsonSerializer.Serialize(o);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            return data;
        }
        public static async Task<T> DeserializeTo<T>(HttpResponseMessage response)
        {
            var str = await response.Content.ReadAsStringAsync();
            return DeserializeTo<T>(str);


        }
        public static T DeserializeTo<T>(string str)
        {
            return JsonSerializer.Deserialize<T>(str, jsonSerializerOptions);
        }

        public static async Task<HttpResponseMessage> Login(this HttpClient client, string email, string password)
        {
            object dto = new
            {
                EmailAddress = email,
                Password = password
            };

            var data = ToStringContent(dto);

            HttpRequestMessage message = new HttpRequestMessage();
            message.Content = data;

            message.Method = HttpMethod.Post;
            message.RequestUri = new Uri("/v1/users/login", UriKind.Relative);

            var response = await client.SendAsync(message);
            return response;
        }
        public static async Task<string?> LoginAndGetToken(this HttpClient client, string email, string password)
        {
            LoginDTO dto = new LoginDTO()
            {
                EmailAddress = email,
                Password = password
            };

            var data = Helper.ToStringContent(dto);
            var response = await client.PostAsync("/v1/users/login", data);

            var content = await response.Content.ReadAsStringAsync();

            var resultLogin = DeserializeTo<ResponseDTO<UserLoginResultDTO>>(content);

            //var resultLogin = Helper.DeserializeTo<ResponseDTO<UserLoginResultDTO>>(content);

            if (resultLogin.StatusCode == 400) return null;
            return resultLogin!.Message.Token;
        }

        public static async Task<HttpResponseMessage> CallWithToken(this HttpClient client, string url, HttpMethod method, dynamic? data, string token)
        {
            var parsedData = data == null ? null : ToStringContent(data);
            var response = await CallWithToken(client, url, method, parsedData, token);
            return response;
        }

        public static async Task<HttpResponseMessage> CallWithToken(this HttpClient client, string url, HttpMethod method, StringContent? data, string token)
        {
            HttpRequestMessage message = new HttpRequestMessage();
            if (data != null) message.Content = data;

            message.Method = method;
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            message.RequestUri = new Uri(url, UriKind.Relative);

            var response = await client.SendAsync(message);
            return response;
        }

        public static async Task<HttpResponseMessage> CallWithQuery(this HttpClient client, string url, HttpMethod method, Dictionary<string, string?>? data, string token)
        {
            HttpRequestMessage message = new HttpRequestMessage();

            message.Method = method;
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (data == null)
            {
                var uri = new Uri(url, UriKind.Relative);
                message.RequestUri = uri;
            }
            else
            {
                var uri = new Uri(QueryHelpers.AddQueryString(url, data.Where(x => x.Value != null)), UriKind.Relative);
                message.RequestUri = uri;
            }

            var response = await client.SendAsync(message);
            return response;
        }

        //public Task ClearDatabase(UnitOfWork unitOfWork)
        //{
        //    await unitOfWork.EnsureDeleted();
        //}
    }
}