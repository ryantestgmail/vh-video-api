using VideoApi.Contract.Responses;
using VideoApi.Domain;

namespace Video.API.Mappings
{
    public class ConferenceToDetailsResponseMapper
    {
        public ConferenceDetailsResponse MapConferenceToResponse(Conference conference)
        {
            var response = new ConferenceDetailsResponse
            {
                Id = conference.Id,
                CaseType = conference.CaseType,
                CaseNumber = conference.CaseNumber,
                CaseName = conference.CaseName,
                ScheduledDateTime = conference.ScheduledDateTime,
                ScheduledDuration = conference.ScheduledDuration,
                CurrentStatus = conference.GetCurrentStatus(),
                Participants =
                    new ParticipantToDetailsResponseMapper().MapParticipantsToResponse(conference.GetParticipants()),
                MeetingRoom = new MeetingRoomToResponseMapper().MapVirtualCourtToResponse(conference.GetMeetingRoom())
            };
            return response;
        }
    }
}