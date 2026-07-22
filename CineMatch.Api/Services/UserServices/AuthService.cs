using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.AuthDto;
using CineMatch.Api.Data.DTO.TokensDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Model;
using CineMatch.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CineMatch.Api.Services.UserServices
{
    public class AuthService : IAuthService
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IJwtTokenService jwtTokenService,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenService refreshTokenService,
            ILogger<AuthService> logger)
        {
            _jwtTokenService = jwtTokenService;
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<BaseResponseWithDataDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Conflict,
                    ResponseMessage = "Username already exists.",
                    Data = null
                };
            }

            var newUser = CreateUserEntity(dto);
            var passwordValidationResult = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, newUser, dto.Password);
            if (!passwordValidationResult.Succeeded)
            {
                var errorMessages = string.Join(", ", passwordValidationResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Password validation failed for {UserName}. Errors: {Errors}", newUser.UserName, errorMessages);
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Password does not meet the requirements.",
                    Errors = errorMessages,
                    Data = null
                };
            }

            var result = await _userManager.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User registration failed for {UserName}. Errors: {Errors}", newUser.UserName, errorMessages);
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = "Something went wrong. There is a server-side problem please try again later.",
                    Errors = errorMessages,
                    Data = null
                };
            }
            _logger.LogInformation("User {UserName} registered successfully.", newUser.UserName);

            // TODO: assign default user role after roles implementation

            var passwordValid = await _userManager.CheckPasswordAsync(newUser, dto.Password);

            var tokens = await CreateTokens(newUser);
            if (tokens == null)
            {
                _logger.LogError("Token generation failed for user {UserName}.", newUser.UserName);
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = "Something went wrong. There is a server-side problem; please try again later.",
                    Data = null
                };
            }
            _logger.LogInformation("Tokens generated successfully for user {UserName}.", newUser.UserName);

            return new BaseResponseWithDataDto<TokensResponseDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "User registered successfully.",
                Data = tokens
            };
        }

        public async Task<BaseResponseWithDataDto<TokensResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = "Invalid username or password."
                };
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordValid)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = "Invalid username or password."
                };
            }

            var tokens = await CreateTokens(user);
            if (tokens == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = "Something went wrong. There is a server-side problem; please try again later.",
                    Data = null
                };
            }

            return new BaseResponseWithDataDto<TokensResponseDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "User logged in successfully.",
                Data = tokens
            };
        }


        public async Task<BaseResponseWithDataDto<TokensResponseDto>> RefreshUserTokensAsync(RefreshTokenRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "User not found.",
                    Data = null
                };
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var tokensRefreshDto = new TokensRefreshDto
            {
                UserId = dto.UserId,
                OldRefreshToken = dto.OldRefreshToken,
                UserName = user.UserName,
                UserRoles = userRoles.ToArray()
            };
            var tokens = await RefreshTokens(tokensRefreshDto);
            if (tokens == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = "Something went wrong. There is a server-side problem; please try again later.",
                    Data = null
                };
            }
            return new BaseResponseWithDataDto<TokensResponseDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Tokens refreshed successfully.",
                Data = tokens
            };
        }



        //private methods
        private async Task<TokensResponseDto?> RefreshTokens(TokensRefreshDto dto)
        {
            var refreshToken = new RefreshTokenRequestDto
            {
                UserId = dto.UserId,
                OldRefreshToken = dto.OldRefreshToken
            };
            var newRefreshToken = await _refreshTokenService.RefreshToken(refreshToken);
            if (newRefreshToken == null)
            {
                return null;
            }

            var accessToken = new AccessTokenDto
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

            var accessTokenRequest = new AccessTokenDto
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