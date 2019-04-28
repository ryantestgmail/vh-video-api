using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Video.API.Mappings;
using VideoApi.Contract.Requests;
using VideoApi.Contract.Responses;
using VideoApi.DAL.Commands;
using VideoApi.DAL.Commands.Core;
using VideoApi.DAL.Exceptions;
using VideoApi.DAL.Queries;
using VideoApi.DAL.Queries.Core;
using VideoApi.Domain;
using VideoApi.Domain.Enums;

namespace Video.API.Controllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("conferences")]
    public class AlertsController : ControllerBase
    {
        private readonly IQueryHandler _queryHandler;
        private readonly ICommandHandler _commandHandler;

        public AlertsController(IQueryHandler queryHandler, ICommandHandler commandHandler)
        {
            _queryHandler = queryHandler;
            _commandHandler = commandHandler;
        }
        
        [HttpPost("{conferenceId}/alerts")]
        [SwaggerOperation(OperationId = "AddAlertToConference")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddAlertToConference(Guid conferenceId, [FromBody] AddAlertRequest request)
        {
            var command = new AddAlertCommand(conferenceId, request.Body, request.Type.GetValueOrDefault());
            try
            {
                await _commandHandler.Handle(command);
            }
            catch (ConferenceNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
        
        [HttpGet("{conferenceId}/alerts")]
        [SwaggerOperation(OperationId = "GetPendingAlerts")]
        [ProducesResponseType(typeof(List<AlertResponse>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetPendingAlerts(Guid conferenceId)
        {
            var query = new GetIncompleteAlertsForConferenceQuery(conferenceId);
            try
            {
                var alerts = await _queryHandler.Handle<GetIncompleteAlertsForConferenceQuery, List<Alert>>(query);
                var mapper = new AlertToResponseMapper();
                var response = alerts.Select(mapper.MapAlertToResponse);
                return Ok(response);
            }
            catch (ConferenceNotFoundException)
            {
                return NotFound();
            }

        }

        [HttpPatch("{conferenceId}/alerts/{alertid}")]
        [SwaggerOperation(OperationId = "UpdateAlertStatus")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAlertStatus(Guid conferenceId, long alertId, [FromBody] UpdateAlertRequest updateAlertRequest)
        {
            var command = new UpdateAlertCommand(conferenceId, alertId, updateAlertRequest.UpdatedBy);
            try
            {
                await _commandHandler.Handle(command);
            }
            catch (AlertNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}