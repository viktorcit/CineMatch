using CineMatch.Api.Data.DTO.RequestsDto.Session;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces.ISessionServices;
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

        [HttpPost("like")]
        public async Task<ActionResult> LikeFilmsAsync(VoteRequestDto dto)
        {
            _logger.LogInformation("лайк фильма");
            var result = await _voteService.LikeFilmsAsync(dto.ClientId, dto.MovieId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [HttpPost("dislike")]
        public async Task<ActionResult> DislikeFilmsAsync(VoteRequestDto dto)
        {
            _logger.LogInformation("дизлайк фильма");
            var result = await _voteService.DislikeFilmsAsync(dto.ClientId, dto.MovieId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [HttpPost("clear-votes/{clientId}")]
        public async Task<ActionResult> ClearSessionVotesAsync(string clientId)
        {
            _logger.LogInformation("очистить голоса сессии");
            var result = await _voteService.ClearSessionVotesAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
