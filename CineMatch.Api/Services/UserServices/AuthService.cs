using CineMatch.Api.Data;
using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.RequestsDto.Auth;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.Tokens;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IJwtServices;
using CineMatch.Api.Services.Interfaces.IUserServices;
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CineMatch.Api.Services.UserServices
{
    public class AuthService : IAuthService
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;
        private readonly AppDbContext _db;

        public AuthService(
            IJwtTokenService jwtTokenService,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenService refreshTokenService,
            ILogger<AuthService> logger,
            AppDbContext db)
        {
            _jwtTokenService = jwtTokenService;
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
            _db = db;
        }

        public async Task<BaseResponseDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null)
            {
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.Conflict, $"User with username '{dto.UserName}' already exists.");
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var newUser = CreateUserEntity(dto);
                var result = await _userManager.CreateAsync(newUser, dto.Password);
                if (!result.Succeeded)
                {
                    var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("User registration failed for {UserName}. Errors: {Errors}", newUser.UserName, errorMessages);

                    var errorMessagesList = result.Errors.FirstOrDefault();
                    var handlerResponse = IdentityErrorHandler.ErrorHandler(errorMessagesList);
                    await transaction.RollbackAsync();
                    return ResponseFactory.Fail<TokensResponseDto>(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(newUser, RolesName.User);
                if (!addToRoleResult.Succeeded)
                {
                    var roleErrorMessages = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Adding user {UserName} to role failed. Errors: {Errors}", newUser.UserName, roleErrorMessages);

                    var errorMessagesList = addToRoleResult.Errors.FirstOrDefault();
                    var handlerResponse = IdentityErrorHandler.ErrorHandler(errorMessagesList);
                    await transaction.RollbackAsync();
                    return ResponseFactory.Fail<TokensResponseDto>(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
                }

                var tokens = await CreateTokens(newUser);
                if (tokens == null)
                {
                    _logger.LogError("Token generation failed for user {UserName}.", newUser.UserName);

                    await transaction.RollbackAsync();
                    return ResponseFactory.Fail<TokensResponseDto>(ErrorType.ServerError, ResponseMessages.ServerError);
                }

                await transaction.CommitAsync();

                _logger.LogInformation("User {UserName} registered successfully.", newUser.UserName);
                return ResponseFactory.Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user registration.");
                await transaction.RollbackAsync();
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.ServerError, ResponseMessages.ServerError);
            }
        }

        public async Task<BaseResponseDto<TokensResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.Unauthorized, ResponseMessages.Unauthorized);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.Unauthorized, ResponseMessages.Unauthorized);
            }

            var tokens = await CreateTokens(user);
            if (tokens == null)
            {
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            return ResponseFactory.Ok(tokens);
        }


        public async Task<BaseResponseDto<TokensResponseDto>> RefreshUserTokensAsync(string userId, string oldRefreshToken)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {      
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.NotFound, $"User with ID '{userId}' not found.");
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var tokensRefreshDto = new TokensRefreshInfo
            {
                UserId = userId,
                OldRefreshToken = oldRefreshToken,
                UserName = user.UserName,
                UserRoles = userRoles.ToArray()
            };
            var tokens = await RefreshTokens(tokensRefreshDto);
            if (tokens == null)
            {
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            return ResponseFactory.Ok(tokens);
        }



        //private methods
        private async Task<TokensResponseDto?> RefreshTokens(TokensRefreshInfo dto)
        {
            var newRefreshToken = await _refreshTokenService.RefreshToken(dto.OldRefreshToken);
            if (newRefreshToken == null)
            {
                return null;
            }

            var accessToken = new AccessTokenInfo
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                UserRoles = dto.UserRoles.ToArray()
            };
            var newAccessToken = _jwtTokenService.GenerateAccessToken(accessToken);
            if (newAccessToken == null)
            {
                return null;
            }

            var response = new TokensResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return response;
        }

        private async Task<TokensResponseDto?> CreateTokens(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var accessTokenRequest = new AccessTokenInfo
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserRoles = roles.ToArray()
            };

            var accessToken = _jwtTokenService.GenerateAccessToken(accessTokenRequest);
            if (accessToken == null)
            {
                return null;
            }

            var refreshToken = await _refreshTokenService.CreateRefreshToken(user.Id);
            if (refreshToken == null)
            {
                return null;
            }

            var response = new TokensResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return response;
        }


        //private static methods
        private static ApplicationUser CreateUserEntity(RegisterRequestDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                CreatedAt = DateTime.UtcNow
            };
            return user;
        }
    }
}