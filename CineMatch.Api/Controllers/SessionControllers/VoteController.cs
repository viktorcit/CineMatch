using CineMatch.Api.Data.DTO.RequestsDto.Session;
using CineMatch.Api.Enums;
using CineMatch.Api.Extensions;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers.SessionControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController : ControllerBase
    {
        private readonly ILogger<VoteController> _logger;
        private readonly IVoteService _voteService;
        public VoteController(ILogger<VoteController> logger, IVoteService voteService)
        {
            _logger = logger;
            _voteService = voteService;
        }

        [Authorize]
        [HttpPost("like")]
        public async Task<ActionResult> LikeFilmsAsync(VoteRequestDto dto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("лайк фильма");
            var result = await _voteService.LikeFilmsAsync(userId, dto.MovieId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpPost("dislike")]
        public async Task<ActionResult> DislikeFilmsAsync(VoteRequestDto dto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("дизлайк фильма");
            var result = await _voteService.DislikeFilmsAsync(userId, dto.MovieId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpPost("clear-votes")]
        public async Task<ActionResult> ClearSessionVotesAsync()
        {
            var userId = User.GetUserId();
            _logger.LogInformation("очистить голоса сессии");
            var result = await _voteService.ClearSessionVotesAsync(userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
