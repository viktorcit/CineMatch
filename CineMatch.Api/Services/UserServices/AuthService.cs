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

namespace CineMatch.Api.Services.UserServices
{
    public class AuthService : IAuthService
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;
        private readonly AppDbContext _db;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(
            IJwtTokenService jwtTokenService,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenService refreshTokenService,
            ILogger<AuthService> logger,
            AppDbContext db,
            SignInManager<ApplicationUser> signInManager)
        {
            _jwtTokenService = jwtTokenService;
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
            _db = db;
            _signInManager = signInManager;
        }

        public async Task<BaseResponseDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "RegisterAsync" });
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null)
            {
                _logger.LogDebug("Registration rejected because the username is already registered");
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.Conflict, $"User with username '{dto.UserName}' already exists.");
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var newUser = CreateUserEntity(dto);
                var result = await _userManager.CreateAsync(newUser, dto.Password);
                if (!result.Succeeded)
                {
                    var errorMessages = string.Join(", ", result.Errors.Select(e => e.Code));

                    var errorMessagesList = result.Errors.FirstOrDefault();
                    var handlerResponse = IdentityErrorHandler.ErrorHandler(errorMessagesList);
                    _logger.Log(handlerResponse.ErrorType == ErrorType.ServerError ? LogLevel.Error : LogLevel.Debug, "User creation rejected by Identity. Codes: {ErrorCodes}", errorMessages);
                    await transaction.RollbackAsync();
                    return ResponseFactory.Fail<TokensResponseDto>(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(newUser, RolesName.User);
                if (!addToRoleResult.Succeeded)
                {
                    var roleErrorMessages = string.Join(", ", addToRoleResult.Errors.Select(e => e.Code));
                    _logger.LogError("Registration cannot complete: assigning role {Role} to user {UserId} failed. Identity codes: {ErrorCodes}", RolesName.User, newUser.Id, roleErrorMessages);

                    var errorMessagesList = addToRoleResult.Errors.FirstOrDefault();
                    var handlerResponse = IdentityErrorHandler.ErrorHandler(errorMessagesList);
                    await transaction.RollbackAsync();
                    return ResponseFactory.Fail<TokensResponseDto>(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
                }

                var tokens = await CreateTokens(newUser);
                if (tokens == null)
                {
                    _logger.LogError("Registration cannot complete: token generation failed for user {UserId}", newUser.Id);

                    await transaction.RollbackAsync();
                    return ResponseFactory.Fail<TokensResponseDto>(ErrorType.ServerError, ResponseMessages.ServerError);
                }

                await transaction.CommitAsync();

                _logger.LogInformation("User {UserId} registered successfully", newUser.Id);
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
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "LoginAsync" });
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                _logger.LogWarning("Sign-in rejected: account not found");
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.Unauthorized, ResponseMessages.Unauthorized);
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!signInResult.Succeeded)
            {
                _logger.LogWarning("Sign-in failed for user {UserId}. LockedOut: {LockedOut}, NotAllowed: {NotAllowed}, RequiresTwoFactor: {RequiresTwoFactor}", user.Id, signInResult.IsLockedOut, signInResult.IsNotAllowed, signInResult.RequiresTwoFactor);
                if (signInResult.IsLockedOut)
                {
                    return ResponseFactory.Fail<TokensResponseDto>(ErrorType.Unauthorized, "Your account is temporarily locked. Please try again later.");
                }
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.Unauthorized, ResponseMessages.Unauthorized);
            }

            var tokens = await CreateTokens(user);
            if (tokens == null)
            {
                _logger.LogError("Sign-in could not issue tokens for user {UserId}", user.Id);
                return ResponseFactory.Fail<TokensResponseDto>(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            _logger.LogInformation("User {UserId} signed in successfully", user.Id);
            return ResponseFactory.Ok(tokens);
        }


        public async Task<BaseResponseDto<TokensResponseDto>> RefreshUserTokensAsync(string userId, string oldRefreshToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "RefreshUserTokensAsync" });
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