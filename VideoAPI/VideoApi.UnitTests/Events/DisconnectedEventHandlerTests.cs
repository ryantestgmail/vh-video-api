using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VideoApi.DAL.Commands;
using VideoApi.Domain.Enums;
using VideoApi.Events.Handlers;
using VideoApi.Events.Models;

namespace VideoApi.UnitTests.Events
{
    public class DisconnectedEventHandlerTests : EventHandlerTestBase
    {
        private DisconnectedEventHandler _eventHandler;

        [Test]
        public async Task should_send_disconnect_messages_to_participants_and_service_bus_on_participant_disconnect()
        {
            _eventHandler = new DisconnectedEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object,
                ServiceBusQueueClient, EventHubContextMock.Object);

            var conference = TestConference;
            var participantForEvent = conference.GetParticipants().First(x => x.UserRole == UserRole.Individual);
            var callbackEvent = new CallbackEvent
            {
                EventType = EventType.Disconnected,
                EventId = Guid.NewGuid().ToString(),
                ParticipantId = participantForEvent.Id,
                ConferenceId = conference.Id,
                Reason = "Unexpected drop",
                TimeStampUtc = DateTime.UtcNow
            };
            var updateStatusCommand = new UpdateParticipantStatusCommand(conference.Id, participantForEvent.Id,
                ParticipantState.Disconnected);
            CommandHandlerMock.Setup(x => x.Handle(updateStatusCommand));

            await _eventHandler.HandleAsync(callbackEvent);

            // Verify messages sent to event hub clients
            EventHubClientMock.Verify(
                x => x.ParticipantStatusMessage(participantForEvent.Username, ParticipantState.Disconnected),
                Times.Exactly(conference.GetParticipants().Count));

            CommandHandlerMock.Verify(
                x => x.Handle(It.Is<UpdateParticipantStatusCommand>(command =>
                    command.ConferenceId == conference.Id &&
                    command.ParticipantId == participantForEvent.Id &&
                    command.ParticipantState == ParticipantState.Disconnected)), Times.Once);
        }

        [Test]
        public async Task
            should_send_disconnect_and_suspend_messages_to_participants_and_service_bus_on_judge_disconnect()
        {
            _eventHandler = new DisconnectedEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object,
                ServiceBusQueueClient, EventHubContextMock.Object);

            var conference = TestConference;
            var participantCount = conference.GetParticipants().Count;
            var participantForEvent = conference.GetParticipants().First(x => x.UserRole == UserRole.Judge);
            var callbackEvent = new CallbackEvent
            {
                EventType = EventType.Disconnected,
                EventId = Guid.NewGuid().ToString(),
                ParticipantId = participantForEvent.Id,
                ConferenceId = conference.Id,
                TimeStampUtc = DateTime.UtcNow
            };
            var updateStatusCommand = new UpdateParticipantStatusCommand(conference.Id, participantForEvent.Id,
                ParticipantState.Disconnected);
            CommandHandlerMock.Setup(x => x.Handle(updateStatusCommand));


            await _eventHandler.HandleAsync(callbackEvent);
            // Verify messages sent to event hub clients
            EventHubClientMock.Verify(
                x => x.ParticipantStatusMessage(_eventHandler.SourceParticipant.Username,
                    ParticipantState.Disconnected),
                Times.Exactly(participantCount));

            EventHubClientMock.Verify(
                x => x.ConferenceStatusMessage(conference.HearingRefId, ConferenceState.Suspended),
                Times.Exactly(participantCount));

            CommandHandlerMock.Verify(
                x => x.Handle(It.Is<UpdateParticipantStatusCommand>(command =>
                    command.ConferenceId == conference.Id &&
                    command.ParticipantId == participantForEvent.Id &&
                    command.ParticipantState == ParticipantState.Disconnected)), Times.Once);

            // Verify messages sent to ASB queue
            ServiceBusQueueClient.Count.Should().Be(1);

            var hearingEventMessage = ServiceBusQueueClient.ReadMessageFromQueue();
            hearingEventMessage.Should().BeOfType<HearingEventMessage>();
            ((HearingEventMessage) hearingEventMessage).ConferenceStatus.Should().Be(ConferenceState.Suspended);
        }
    }
}