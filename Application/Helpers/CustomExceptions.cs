using Application.DTOs;
using System.Text.Json;

namespace Application.Helpers
{
    public static class ErrorNameValues
    {
        public static string UserDuplicated => "USER_DUPLICATED";
        public static string UserNotFound => "USER_NOT_FOUND";
        public static string UserUnableToChangePassword => "USER_UNABLE_TO_CHANGE_PASSWORD";

        public static string ProjectNotAvailable => "PROJECT_NOT_AVAILABLE";

        public static string TooMuchParams => "TOO_MUCH_PARAMS";
        public static string InvalidPincode => "INVALID_PINCODE";
        public static string PinCooldown => "PIN_COOLDOWN";
        public static string AlreadyInProject => "ALREADY_IN_PROJECT";

        public static string InvalidCredential => "INVALID_CREDENTIAL";
        public static string WrongLoginType => "WRONG_LOGIN_TYPE";
        public static string WrongValue => "WRONG_VALUE";

        public static string ProjectNotFound => "PROJECT_NOT_FOUND";
        public static string ProjectIsPrivate => "PROJECT_NOT_FOUND";

        public static string ProjectMemberNotFound => "PROJECT_MEMBER_NOT_FOUND";

        public static string MemberNotFound => "MEMBER_NOT_FOUND";
        public static string MemberNotAvailable => "MEMBER_NOT_AVAILALBE";
        public static string BadImage => "BAD_IMAGE";

        public static string RequestNotFound => "REQUEST_NOT_FOUND";
        public static string RequestDuplicated => "REQUEST_DUPLICATED";
        public static string RequestSalaryCycleAccepted => "REQUEST_SALARY_CYLCE_ACCEPTED";

        public static string EmailDuplicated => "EMAIL_DUPLICATED";
        public static string DafuqMembers => "DAFUQ_MEMBERS";

        public static string SalaryCycleNotFound => "SALARY_CYCLE_NOT_FOUND";
        public static string SalaryCycleExist => "SALARY_CYCLE_EXIST";
        public static string SalaryCycleNotAvailable => "SALARY_CYCLE_NOT_AVAILABLE";

        public static string MissingMembers => "MISSING_MEMBERS";
        public static string InvalidParameters => "INVALID_PARAMETERS";

        public static string ExceedLimit => "EXCEED_LIMIT";

        public static string SupplierNotFound => "SUPPLIER_NOT_FOUND";
        public static string SupplierHadVoucher => "SUPPLIER_HAD_VOUCHER";

        public static string NoPermission => "NO_PERMISSION";

        public static string InvalidStateChange => "INVALID_STATE_CHANGE";
        public static string ProjectUnavailable => "PROJECT_UNAVAIBLABLE";

        public static string ReportNotFound => "REPORT_NOT_FOUND";
        public static string ReportNotUpdatable => "REPORT_NOT_UPDATEABLE";

        public static string InsufficentToken => "INSUFFICENT_TOKEN";

        public static string RequestNotAvailableToBeUpdate => "REQUEST_NOT_AVAIBLE_TO_BE_UPDATE";

        public static string ServerError => "SERVER_ERROR";
        public static string SystemError => "SYSTEM_...";

        public static string SponsorExisted => "SPONSOR_EXISTED";
        public static string SponsorNameExisted => "SPONSOR_NAME_EXISTED";
        public static string SponsorNotFound => "SPONSOR_NOT_FOUND";

        public static string LevelNotFound => "LEVEL_NOT_FOUND";
        public static string LevelDuplicated => "LEVEL_DUPLICATED";

        public static string FileSizeTooBig => "FILE_SIZE_TOO_BIG";
        public static string FileWrongType => "FILE_WRONG_TYPE";

        public static string VoucherNotFound => "VOUCHER_NOT_FOUND";

        public static string NotificationNotFound => "NOTIFICATION_NOT_FOUND";

        public static string MilestoneNotFound => "MILESTONE_NOT_FOUND";

        public static string MemberVoucherNotFound => "MEMBER_VOUCHER_NOT_FOUND";
        public static string MemberVoucherUsed => "MEMBER_VOUCHER_USED";
        public static string MemberVoucherNotYour => "MEMBER_VOUCHER_NOT_YOUR";

    }

    public class ExceptionResolveMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionResolveMiddleware> _logger;

        public ExceptionResolveMiddleware(RequestDelegate next, ILogger<ExceptionResolveMiddleware> logger)
        {
            _next = next;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            string errorName = "UNKNOWN";
            string? error = null;

            try
            {
                await _next.Invoke(context);
            }
            catch (NotFoundException ex)
            {
                error = ex.ErrorMsg;
                errorName = ex.ErrorName!;
                context.Response.StatusCode = 404;
            }
            catch (BadRequestException ex)
            {
                error = ex.ErrorMsg;
                errorName = ex.ErrorName!;
                context.Response.StatusCode = 400;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                // errors = ex.Message;
                context.Response.StatusCode = 418;
                error = ex.StackTrace ?? "";
            }

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";

                var response = new ResponseDTO<bool>()
                {
                    IsSuccess = false,
                    StatusCode = context.Response.StatusCode,
                    Message = false,
                    ErrorName = errorName,
                    ErrorMsg = error
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));

            }
        }
    }

    public class CustomException : Exception
    {
        public CustomException()
        {
        }

        public string? ErrorName { get; set; }
        public string? ErrorMsg { get; set; }

        public CustomException(string message)
        {
            ErrorMsg = message;
        }

        public CustomException(string message, string customErrorName)
            : base(message)
        {
            ErrorName = customErrorName;
            ErrorMsg = message;
        }
    }

    public class NotFoundException : CustomException
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message) : base(message, "NOT_FOUND")
        {
        }

        public NotFoundException(string message, string customErrorName)
            : base(message, customErrorName)
        {
        }
    }

    public class BadRequestException : CustomException
    {
        public BadRequestException()
        {
        }

        public BadRequestException(string message) : base(message, "BAD_REQUEST")
        {
        }

        public BadRequestException(string message, string customErrorName)
            : base(message, customErrorName)
        {

        }
    }
}