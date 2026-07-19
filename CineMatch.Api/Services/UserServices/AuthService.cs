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

            var result = await _userManager.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = "Something went wrong. There is a server-side problem; please try again later.",
                    Data = null
                };
            }

            // TODO: assign default user role after roles implementation

            var tokens = await CreateTokens(newUser);
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


        public async Task<BaseResponseWithDataDto<TokensResponseDto>> ReplaceRefreshAndAccessTokensAsync(RefreshTokenRequestDto dto)
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

            var accessToken = new AccessTokenRequestDto
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

            var accessTokenRequest = new AccessTokenRequestDto
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