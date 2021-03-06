using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using VideoApi.DAL;
using VideoApi.DAL.Commands;
using VideoApi.DAL.Exceptions;
using VideoApi.DAL.Queries;
using VideoApi.Domain.Enums;

namespace VideoApi.IntegrationTests.Database.Commands
{
    public class CloseConferenceCommandTests : DatabaseTestsBase
    {
        private CloseConferenceCommandHandler _handler;
        private GetConferenceByIdQueryHandler _conferenceByIdHandler;
        private Guid _newConferenceId;

        [SetUp]
        public void Setup()
        {
            var context = new VideoApiDbContext(VideoBookingsDbContextOptions);
            _handler = new CloseConferenceCommandHandler(context);
            _conferenceByIdHandler = new GetConferenceByIdQueryHandler(context);
            _newConferenceId = Guid.Empty;
        }

        [Test]
        public void should_throw_exception_when_conference_does_not_exist()
        {
            var conferenceId = Guid.NewGuid();
            var command = new CloseConferenceCommand(conferenceId);
            Assert.ThrowsAsync<ConferenceNotFoundException>(() => _handler.Handle(command));
        }
        
        [Test]
        public async Task should_update_conference_to_closed()
        {
            var beforeActionTime = DateTime.UtcNow;
            var seededConference = await TestDataManager.SeedConference();
            TestContext.WriteLine($"New seeded conference id: {seededConference.Id}");
            _newConferenceId = seededConference.Id;

            var beforeCount = seededConference.GetConferenceStatuses().Count;
            var beforeState = seededConference.GetCurrentStatus();

            var command = new CloseConferenceCommand(_newConferenceId);
            await _handler.Handle(command);

            var updatedConference = await _conferenceByIdHandler.Handle(new GetConferenceByIdQuery(_newConferenceId));
            var afterCount = updatedConference.GetConferenceStatuses().Count;
            var afterState = updatedConference.GetCurrentStatus();

            afterCount.Should().BeGreaterThan(beforeCount);
            afterState.Should().NotBe(beforeState);
            afterState.Should().Be(ConferenceState.Closed);

            updatedConference.IsClosed().Should().BeTrue();
            updatedConference.ClosedDateTime.Should().BeAfter(beforeActionTime);
        }


        [TearDown]
        public async Task TearDown()
        {
            if (_newConferenceId != Guid.Empty)
            {
                TestContext.WriteLine($"Removing test conference {_newConferenceId}");
                await TestDataManager.RemoveConference(_newConferenceId);
            }
        }
    }
}