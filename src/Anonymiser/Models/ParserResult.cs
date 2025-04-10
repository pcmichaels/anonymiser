using System;

namespace Anonymiser.Models
{
    public class ParserResult<T>
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        public T? Data { get; }

        private ParserResult(bool isSuccess, string? errorMessage, T? data)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Data = data;
        }

        public static ParserResult<T> Success(T data)
        {
            return new ParserResult<T>(true, string.Empty, data);
        }

        public static ParserResult<T> Failure(string errorMessage)
        {
            return new ParserResult<T>(false, errorMessage, default);
        }
    }
} 