using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Testing.Common.Helper.Builders.Domain;
using VideoApi.DAL.Commands.Core;
using VideoApi.DAL.Queries;
using VideoApi.DAL.Queries.Core;
using VideoApi.Domain;
using VideoApi.Domain.Enums;
using VideoApi.Events.Handlers;
using VideoApi.Events.Handlers.Core;
using VideoApi.UnitTests.Stubs;

namespace VideoApi.UnitTests.Events
{
    public abstract class EventHandlerTestBase
    {
        protected Mock<ICommandHandler> CommandHandlerMock;
        protected List<IEventHandler> EventHandlersList;
        protected Mock<IQueryHandler> QueryHandlerMock;
        protected ServiceBusQueueClientStub ServiceBusQueueClient;

        protected Conference TestConference;

        [SetUp]
        public void Setup()
        {
            QueryHandlerMock = new Mock<IQueryHandler>();
            CommandHandlerMock = new Mock<ICommandHandler>();
            ServiceBusQueueClient = new ServiceBusQueueClientStub();

            EventHandlersList = new List<IEventHandler>
            {
                new CloseEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new DisconnectedEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new JoinedEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new JudgeAvailableEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object,
                    ServiceBusQueueClient),
                new LeaveEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new PauseEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new SuspendEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new TransferEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new ParticipantJoiningEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
                new SelfTestFailedEventHandler(QueryHandlerMock.Object, CommandHandlerMock.Object, ServiceBusQueueClient),
            };

            TestConference = new ConferenceBuilder()
                .WithParticipant(UserRole.Judge, null)
                .WithParticipant(UserRole.Individual, "Claimant")
                .WithParticipant(UserRole.Representative, "Claimant")
                .WithParticipant(UserRole.Individual, "Defendant")
                .WithParticipant(UserRole.Representative, "Defendant")
                .Build();

            QueryHandlerMock
                .Setup(x => x.Handle<GetConferenceByIdQuery, Conference>(It.IsAny<GetConferenceByIdQuery>()))
                .ReturnsAsync(TestConference);
        }
    }
}