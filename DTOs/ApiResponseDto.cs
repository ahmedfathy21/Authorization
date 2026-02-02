namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// Generic DTO for standardized API responses
    /// Wraps all API responses with status, message, and data
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponseDto<T>
    {
        /// <summary>
        /// Indicates whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The actual response data
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// List of errors (if any)
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Constructor for successful response
        /// </summary>
        public ApiResponseDto(T data, string message = "Success")
        {
            Success = true;
            Data = data;
            Message = message;
        }

        /// <summary>
        /// Constructor for error response
        /// </summary>
        public ApiResponseDto(string message, List<string> errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ApiResponseDto()
        {
        }
    }
}
