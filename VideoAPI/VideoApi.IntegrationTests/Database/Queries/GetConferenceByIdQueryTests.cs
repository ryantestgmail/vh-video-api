using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using VideoApi.DAL;
using VideoApi.DAL.Queries;

namespace VideoApi.IntegrationTests.Database.Queries
{
    public class GetConferenceByIdQueryTests : DatabaseTestsBase
    {
        private GetConferenceByIdQueryHandler _handler;
        private Guid _newConferenceId;
        
        [SetUp]
        public void Setup()
        {
            var context = new VideoApiDbContext(VideoBookingsDbContextOptions);
            _handler = new GetConferenceByIdQueryHandler(context);
            _newConferenceId = Guid.Empty;
        }
        
        [Test]
        public async Task should_get_conference_details_by_id()
        {
            var seededConference = await TestDataManager.SeedConference();
            TestContext.WriteLine($"New seeded conference id: {seededConference.Id}");
            _newConferenceId = seededConference.Id;
            var conference = await _handler.Handle(new GetConferenceByIdQuery(_newConferenceId));
            
            conference.Should().NotBeNull();

            conference.CaseType.Should().Be(seededConference.CaseType);
            conference.CaseNumber.Should().Be(seededConference.CaseNumber);
            conference.ScheduledDateTime.Should().Be(seededConference.ScheduledDateTime);
            conference.HearingRefId.Should().Be(seededConference.HearingRefId);

            var participants = conference.GetParticipants();
            participants.Should().NotBeNullOrEmpty();
            foreach (var participant in participants)
            {
                participant.Name.Should().NotBeNullOrEmpty();
                participant.Username.Should().NotBeNullOrEmpty();
                participant.DisplayName.Should().NotBeNullOrEmpty();
                participant.ParticipantRefId.Should().NotBeEmpty();
                participant.HearingRole.Should().NotBeNullOrEmpty();
                participant.CaseTypeGroup.Should().NotBeNullOrEmpty();
            }
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