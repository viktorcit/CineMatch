using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.AuthDto;
using CineMatch.Api.Data.DTO.TokensDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
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

        public async Task<BaseResponseDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                var existingUser = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUser != null)
                {
                    return ErrorFactory.Conflict<TokensResponseDto>($"User with username '{dto.UserName}' already exists.");
                }

                var newUser = CreateUserEntity(dto);
                var passwordValidationResult = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, newUser, dto.Password);
                if (!passwordValidationResult.Succeeded)
                {
                    var errorMessages = string.Join(", ", passwordValidationResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Password validation failed for {UserName}. Errors: {Errors}", newUser.UserName, errorMessages);

                    return ErrorFactory.BadRequest<TokensResponseDto>("Password does not meet the requirements.");
                }


                var result = await _userManager.CreateAsync(newUser, dto.Password);
                if (!result.Succeeded)
                {
                    var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("User registration failed for {UserName}. Errors: {Errors}", newUser.UserName, errorMessages);

                    return ErrorFactory.BadRequest<TokensResponseDto>(errorMessages);
                }
                _logger.LogInformation("User {UserName} registered successfully.", newUser.UserName);

                // TODO: assign default user role after roles implementation

                var tokens = await CreateTokens(newUser);
                if (tokens == null)
                {
                    _logger.LogError("Token generation failed for user {UserName}.", newUser.UserName);
                    return ErrorFactory.ServerError<TokensResponseDto>([]);
                }
                _logger.LogInformation("Tokens generated successfully for user {UserName}.", newUser.UserName);

                return ErrorFactory.Ok(tokens, $"User '{newUser.UserName}' registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user registration.");
                return ErrorFactory.ServerError<TokensResponseDto>([]);
            }
        }

        public async Task<BaseResponseDto<TokensResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                return ErrorFactory.Unauthorized<TokensResponseDto>();
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                return ErrorFactory.Unauthorized<TokensResponseDto>();
            }

            var tokens = await CreateTokens(user);
            if (tokens == null)
            {
                return ErrorFactory.ServerError<TokensResponseDto>([]);
            }

            return ErrorFactory.Ok(tokens, $"User '{user.UserName}' logged in successfully.");
        }


        public async Task<BaseResponseDto<TokensResponseDto>> RefreshUserTokensAsync(RefreshTokenRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {      
                return ErrorFactory.NotFound<TokensResponseDto>($"User with ID '{dto.UserId}' not found.");
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
                return ErrorFactory.ServerError<TokensResponseDto>([]);
            }
            return ErrorFactory.Ok(tokens, $"Tokens refreshed successfully for user '{user.UserName}'.");
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