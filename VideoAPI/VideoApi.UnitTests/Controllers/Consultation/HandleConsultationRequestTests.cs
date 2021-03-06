using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VideoApi.Contract.Requests;
using VideoApi.Domain.Enums;
using VideoApi.Domain.Validations;
using Task = System.Threading.Tasks.Task;

namespace VideoApi.UnitTests.Controllers.Consultation
{
    public class HandleConsultationRequestTests : ConsultationControllerTestBase
    {
        [Test]
        public async Task should_raise_notification_to_requester_and_admin_when_consultation_is_accepted()
        {
            var conferenceId = TestConference.Id;
            var requestedBy = TestConference.GetParticipants()[2];
            var requestedFor = TestConference.GetParticipants()[3];
            
            var answer = ConsultationAnswer.Accepted;

            var request = new ConsultationRequest
            {
                ConferenceId = conferenceId,
                RequestedBy = requestedBy.Id,
                RequestedFor = requestedFor.Id,
                Answer = answer
            };

            await Controller.HandleConsultationRequest(request);

            VideoPlatformServiceMock.Verify(x =>
                x.StartPrivateConsultationAsync(TestConference, requestedBy, requestedFor), Times.Once);
            VideoPlatformServiceMock.VerifyNoOtherCalls();
        }

        
        [Test]
        public async Task should_return_error_when_consultation_accepted_but_no_room_is_available()
        {
            var conferenceId = TestConference.Id;
            var requestedBy = TestConference.GetParticipants()[2];
            var requestedFor = TestConference.GetParticipants()[3];

            VideoPlatformServiceMock
                .Setup(x => x.StartPrivateConsultationAsync(TestConference, requestedBy, requestedFor))
                .ThrowsAsync(new DomainRuleException("Unavailable room", "No consultation rooms available"));
            
            // make sure no rooms are available
            TestConference.Participants[1].UpdateCurrentRoom(RoomType.ConsultationRoom1);
            TestConference.Participants[4].UpdateCurrentRoom(RoomType.ConsultationRoom2);

            var answer = ConsultationAnswer.Accepted;

            var request = new ConsultationRequest
            {
                ConferenceId = conferenceId,
                RequestedBy = requestedBy.Id,
                RequestedFor = requestedFor.Id,
                Answer = answer
            };

            var result = await Controller.HandleConsultationRequest(request);
            var typedResult = (ObjectResult) result;
            typedResult.StatusCode.Should().Be((int) HttpStatusCode.BadRequest);

            
            VideoPlatformServiceMock.Verify(x =>
                x.StartPrivateConsultationAsync(TestConference, requestedBy, requestedFor), Times.Once);
            VideoPlatformServiceMock.VerifyNoOtherCalls();
        }
    }
}