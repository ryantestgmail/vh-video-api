using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Video.API.Mappings;
using VideoApi.Contract.Requests;
using VideoApi.Contract.Responses;
using VideoApi.DAL.Commands;
using VideoApi.DAL.Commands.Core;
using VideoApi.DAL.Queries;
using VideoApi.DAL.Queries.Core;
using VideoApi.Domain;
using VideoApi.Services;

namespace Video.API.Controllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("conferences")]
    [ApiController]
    public class ParticipantsController : ControllerBase
    {
        private readonly IQueryHandler _queryHandler;
        private readonly ICommandHandler _commandHandler;
        private readonly IVideoPlatformService _videoPlatformService;
        private readonly ILogger<ParticipantsController> _logger;

        public ParticipantsController(ICommandHandler commandHandler, IQueryHandler queryHandler,
            IVideoPlatformService videoPlatformService, ILogger<ParticipantsController> logger)
        {
            _commandHandler = commandHandler;
            _queryHandler = queryHandler;
            _videoPlatformService = videoPlatformService;
            _logger = logger;
        }

        /// <summary>
        /// Add participants to a conference
        /// </summary>
        /// <param name="conferenceId">The id of the conference to add participants to</param>
        /// <param name="request">Details of the participant</param>
        /// <returns></returns>
        [HttpPut("{conferenceId}/participants")]
        [SwaggerOperation(OperationId = "AddParticipantsToConference")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddParticipantsToConference(Guid conferenceId,
            AddParticipantsToConferenceRequest request)
        {
            _logger.LogDebug("AddParticipantsToConference");
            var getConferenceByIdQuery = new GetConferenceByIdQuery(conferenceId);
            var queriedConference =
                await _queryHandler.Handle<GetConferenceByIdQuery, Conference>(getConferenceByIdQuery);

            if (queriedConference == null)
            {
                _logger.LogError($"Unable to find conference {conferenceId}");
                return NotFound();
            }

            var participants = request.Participants.Select(x =>
                    new Participant(x.ParticipantRefId, x.Name.Trim(), x.DisplayName.Trim(),
                        x.Username.ToLowerInvariant().Trim(), x.UserRole,
                        x.CaseTypeGroup)
                    {
                        Representee = x.Representee
                    })
                .ToList();

            var addParticipantCommand = new AddParticipantsToConferenceCommand(conferenceId, participants);
            await _commandHandler.Handle(addParticipantCommand);

            return NoContent();
        }

        /// <summary>
        /// Update participant details
        /// </summary>
        /// <param name="conferenceId">Id of conference to look up</param>
        /// <param name="participantId">Id of participant to remove</param>
        /// <param name="request">The participant information to update</param>
        /// <returns></returns>
        [HttpPatch("{conferenceId}/participants/{participantId}", Name = "UpdateParticipantDetails")]
        [SwaggerOperation(OperationId = "UpdateParticipantDetails")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateParticipantDetails(Guid conferenceId, Guid participantId, UpdateParticipantRequest request)
        {
            _logger.LogDebug("UpdateParticipantDetails");
            var getConferenceByIdQuery = new GetConferenceByIdQuery(conferenceId);
            var queriedConference =
                await _queryHandler.Handle<GetConferenceByIdQuery, Conference>(getConferenceByIdQuery);

            if (queriedConference == null)
            {
                _logger.LogError($"Unable to find conference {conferenceId}");
                return NotFound();
            }

            var participant = queriedConference.Participants.SingleOrDefault(x => x.Id == participantId);
            if (participant == null)
            {
                _logger.LogError($"Unable to find participant {participantId}");
                return NotFound();
            }

            var updateParticipantDetailsCommand = new UpdateParticipantDetailsCommand(conferenceId, participantId, request.Fullname,
                request.DisplayName, request.Representee);
            await _commandHandler.Handle(updateParticipantDetailsCommand);

            return NoContent();
        }

        /// <summary>
        /// Remove participants from a conference
        /// </summary>
        /// <param name="conferenceId">The id of the conference to remove participants from</param>
        /// <param name="participantId">The id of the participant to remove</param>
        /// <returns></returns>
        [HttpDelete("{conferenceId}/participants/{participantId}")]
        [SwaggerOperation(OperationId = "RemoveParticipantFromConference")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveParticipantFromConference(Guid conferenceId, Guid participantId)
        {
            _logger.LogDebug("RemoveParticipantFromConference");
            var getConferenceByIdQuery = new GetConferenceByIdQuery(conferenceId);
            var queriedConference =
                await _queryHandler.Handle<GetConferenceByIdQuery, Conference>(getConferenceByIdQuery);

            if (queriedConference == null)
            {
                _logger.LogError($"Unable to find conference {conferenceId}");
                return NotFound();
            }

            var participant = queriedConference.GetParticipants().SingleOrDefault(x => x.Id == participantId);
            if (participant == null)
            {
                _logger.LogError($"Unable to find participant {participantId}");
                return NotFound();
            }

            var participants = new List<Participant> {participant};
            var command = new RemoveParticipantsFromConferenceCommand(conferenceId, participants);
            await _commandHandler.Handle(command);
            return NoContent();
        }

        /// <summary>
        /// Get the test call result for a participant
        /// </summary>
        /// <param name="conferenceId">The id of the conference</param>
        /// <param name="participantId">The id of the participant</param>
        /// <returns></returns>
        [HttpGet("{conferenceId}/participants/{participantId}/selftestresult")]
        [SwaggerOperation(OperationId = "GetTestCallResultForParticipant")]
        [ProducesResponseType(typeof(TestCallScoreResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTestCallResultForParticipant(Guid conferenceId, Guid participantId)
        {
            _logger.LogDebug("GetTestCallResultForParticipant");
            var testCallResult = await _videoPlatformService.GetTestCallScoreAsync(participantId);
            if (testCallResult == null)
            {
                _logger.LogError(
                    $"Unable to find test call result for participant {participantId} in conference {conferenceId}");
                return NotFound();
            }

            var command = new UpdateSelfTestCallResultCommand(conferenceId, participantId, testCallResult.Passed,
                testCallResult.Score);
            await _commandHandler.Handle(command);
            _logger.LogDebug("Saving test call result");
            var response = new TaskCallResultResponseMapper().MapTaskToResponse(testCallResult);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves the independent self test result without saving it
        /// </summary>
        /// <param name="participantId">The id of the participant</param>
        /// <returns></returns>
        [HttpGet("independentselftestresult")]
        [SwaggerOperation(OperationId = "GetIndependentTestCallResult")]
        [ProducesResponseType(typeof(TestCallScoreResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetIndependentTestCallResult(Guid participantId)
        {
            _logger.LogDebug("GetIndependentTestCallResult");
            var testCallResult = await _videoPlatformService.GetTestCallScoreAsync(participantId);
            if (testCallResult == null)
            {
                _logger.LogError(
                    $"Unable to find test call result for participant {participantId}");
                return NotFound();
            }
            var response = new TaskCallResultResponseMapper().MapTaskToResponse(testCallResult);
            return Ok(response);
        }
    }
}