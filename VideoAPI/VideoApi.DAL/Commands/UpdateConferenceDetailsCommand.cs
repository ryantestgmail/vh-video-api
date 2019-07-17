using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VideoApi.DAL.Commands.Core;
using VideoApi.DAL.Exceptions;
using VideoApi.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace VideoApi.DAL.Commands
{
    public class UpdateConferenceDetailsCommand : ICommand
    {
        public Guid HearingRefId { get; }
        public string CaseType { get; }
        public DateTime ScheduledDateTime { get; }
        public string CaseNumber { get; }
        public string CaseName { get; }
        public int ScheduledDuration { get; }

        public UpdateConferenceDetailsCommand(Guid hearingRefId, string caseNumber, string caseType, 
            string caseName, int duration, DateTime scheduledDateTime)
        {
            HearingRefId = hearingRefId;
            CaseNumber = caseNumber;
            CaseType = caseType;
            CaseName = caseName;
            ScheduledDuration = duration;
            ScheduledDateTime = scheduledDateTime;
        }
    }

    public class UpdateConferenceDetailsCommandHandler : ICommandHandler<UpdateConferenceDetailsCommand>
    {
        private readonly VideoApiDbContext _context;

        public UpdateConferenceDetailsCommandHandler(VideoApiDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateConferenceDetailsCommand command)
        {
            var conference = await _context.Conferences
                .Include("ConferenceStatuses")
                .Where(x => x.GetCurrentStatus() != ConferenceState.Closed)
                .SingleOrDefaultAsync(x => x.HearingRefId == command.HearingRefId);

            if (conference == null)
            {
                throw new ConferenceNotFoundException(command.HearingRefId);
            }

            conference.UpdateConferenceDetails(command.CaseType, command.CaseNumber, command.CaseName,
                command.ScheduledDuration, command.ScheduledDateTime);
            await _context.SaveChangesAsync();
        }
    }
}