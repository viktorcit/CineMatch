using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.UserDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.UserServices
{
    public class AccountService //: IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly AppDbContext _db;

        public AccountService(ILogger<AccountService> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }







        //public async Task<BaseResponseWithDataDto<UserDto>> GetAccountInfo(string accountId)
        //{
        //    if (string.IsNullOrWhiteSpace(accountId))
        //    {
        //        return new BaseResponseWithDataDto<UserDto>
        //        {
        //            IsSuccess = false,
        //            ResponseMessage = "Account ID cannot be empty.",
        //            ErrorType = ErrorType.BadRequest
        //        };
        //    }
        //    var account = await _db.Users.FirstOrDefaultAsync(a => a.PublicId == accountId);
        //    if (account == null)
        //    {
        //        return new BaseResponseWithDataDto<UserDto>
        //        {
        //            IsSuccess = false,
        //            ResponseMessage = "No account found.",
        //            ErrorType = ErrorType.NotFound
        //        };
        //    }
        //    var responseData = new UserDto
        //    {
        //        Id = account.Id,
        //        CreatedAt = account.CreatedAt
        //    };
        //    return new BaseResponseWithDataDto<UserDto>
        //    {
        //        IsSuccess = true,
        //        ResponseMessage = "Account information retrieved successfully.",
        //        ErrorType = ErrorType.None,
        //        Data = responseData
        //    };
        //}


        public async Task<BaseResponseWithDataDto<UserDto>> SwitchAccount(string accountId, string secret)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return new BaseResponseWithDataDto<UserDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Account ID cannot be empty.",
                    ErrorType = ErrorType.BadRequest
                };
            }
            if (string.IsNullOrWhiteSpace(secret))
            {
                return new BaseResponseWithDataDto<UserDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Secret cannot be empty.",
                    ErrorType = ErrorType.BadRequest
                };
            }

            //var account = await _db.Users.FirstOrDefaultAsync();
            //if (account == null)
            //{
            //    return new BaseResponseWithDataDto<UserDto>
            //    {
            //        IsSuccess = false,
            //        ResponseMessage = "Account not found.",
            //        ErrorType = ErrorType.NotFound
            //    };
            //}

            //var responseData = new UserDto
            //{
            //    Id = account.Id,
            //    PublicId = account.PublicId,
            //    Secret = account.Secret,
            //    CreatedAt = account.CreatedAt
            //};


            return new BaseResponseWithDataDto<UserDto>
            {
                IsSuccess = true,
                ResponseMessage = $"Account {accountId} switched successfully.",
                ErrorType = ErrorType.None,
                //Data = responseData
            };
        }
    }
}
