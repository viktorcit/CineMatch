using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.AuthDto;
using CineMatch.Api.Data.TokensDto;
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

        public AuthService(
            IJwtTokenService jwtTokenService,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenService refreshTokenService)
        {
            _jwtTokenService = jwtTokenService;
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<BaseResponseWithDataDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Username and password are required.",
                    Data = null
                };
            }

            var existingUser = await _userManager.FindByUserNameAsync(dto.UserName);

            if (!existingUser)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Username already exists.",
                    Data = null
                };
            }

            var newUser = new ApplicationUser
            {
                UserName = dto.UserName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Failed to create user.",
                    Data = null
                };
            }

            var accessTokenEntity = new AccessTokenRequestDto
            {
                UserId = newUser.Id,
                UserName = newUser.UserName,
                UserRoles = []
            };

            var accessToken = _jwtTokenService.GenerateAccessToken(accessTokenEntity);

            var refreshToken = await _refreshTokenService.CreateRefreshToken(newUser.Id);

            if (accessToken == null || refreshToken == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Server error.",
                    ErrorType = ErrorType.ServerError
                };
            }

            return new BaseResponseWithDataDto<TokensResponseDto>
            {
                IsSuccess = true,
                ResponseMessage = "User registered successfully.",
                Data = new TokensResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            };
        }

        public async Task<BaseResponseWithDataDto<TokensResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Username and password are required."
                };
            }

            var user = await _userManager.FindByUserNameAsync(dto.UserName);

            if (user == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Invalid username or password."
                };
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordValid)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Invalid username or password."
                };
            }

            var accessTokenEntity = new AccessTokenRequestDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserRoles = []
            };

            var accessToken = _jwtTokenService.GenerateAccessToken(accessTokenEntity);

            var refreshToken = await _refreshTokenService.CreateRefreshToken(user.Id);

            if (accessToken == null || refreshToken == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Server error.",
                    ErrorType = ErrorType.ServerError
                };
            }

            var tokensResponse = new TokensResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return new BaseResponseWithDataDto<TokensResponseDto>
            {
                IsSuccess = true,
                ResponseMessage = "User logged in successfully.",
                Data = tokensResponse
            };
        }
    }
}