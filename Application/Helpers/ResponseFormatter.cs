using Application.DTOs;


namespace Application.Helpers
{
    public static class ResponseFormatter
    {
        public static ResponseDTO<K> FormatAsResponseDTO<K>(this K data, int statusCode)
        {
            return new ResponseDTO<K>()
            {
                IsSuccess = IsSuccessStatusCode(statusCode),
                StatusCode = statusCode,
                Message = data
            };
        }

        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode < 400;
        }
    }
}