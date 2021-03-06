using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Testing.Common.Helper.Builders.Domain;
using VideoApi.DAL;
using VideoApi.DAL.Queries;
using VideoApi.Domain;
using VideoApi.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace VideoApi.IntegrationTests.Database.Queries
{
    public class GetConferencesForTodayByUsernameQueryTests : DatabaseTestsBase
    {
        private GetConferencesForTodayByUsernameQueryHandler _handler;
        private Guid _newConferenceId1;
        private Guid _newConferenceId2;
        private Guid _newConferenceId3;
        private Guid _newConferenceId4;
        private Guid _newConferenceId5;
        private Guid _newConferenceId6;

        [SetUp]
        public void Setup()
        {
            var context = new VideoApiDbContext(VideoBookingsDbContextOptions);
            _handler = new GetConferencesForTodayByUsernameQueryHandler(context);
            _newConferenceId1 = Guid.Empty;
            _newConferenceId2 = Guid.Empty;
            _newConferenceId3 = Guid.Empty;
            _newConferenceId4 = Guid.Empty;
            _newConferenceId5 = Guid.Empty;
            _newConferenceId6 = Guid.Empty;
        }

        [Test]
        public async Task should_get_conference_for_username()
        {
            var username = "Automation_knownuser@email.com";
            var conference1 = new ConferenceBuilder(true)
                .WithParticipant(UserRole.Representative, "Defendant", username)
                .WithParticipant(UserRole.Judge, null)
                .WithConferenceStatus(ConferenceState.Closed)
                .Build();
            _newConferenceId1 = conference1.Id;

            var conference2 = new ConferenceBuilder(true)
                .WithParticipant(UserRole.Representative, "Defendant", username)
                .WithParticipant(UserRole.Judge, null)
                .WithConferenceStatus(ConferenceState.InSession)
                .WithHearingTask("Test Task")
                .Build();
            _newConferenceId2 = conference2.Id;

            var conference3 = new ConferenceBuilder(true)
                .WithParticipant(UserRole.Representative, "Defendant", username)
                .WithParticipant(UserRole.Judge, null)
                .WithConferenceStatus(ConferenceState.Paused)
                .Build();
            _newConferenceId3 = conference3.Id;

            var conference4 = new ConferenceBuilder(true)
                .WithParticipant(UserRole.Representative, "Defendant", username)
                .WithParticipant(UserRole.Judge, null)
                .WithConferenceStatus(ConferenceState.Suspended)
                .WithJudgeTask("Test Task")
                .Build();
            _newConferenceId4 = conference4.Id;

            var conference5 = new ConferenceBuilder(true, null, DateTime.UtcNow.AddDays(-1))
                .WithParticipant(UserRole.Representative, "Defendant", username)
                .WithParticipant(UserRole.Judge, null)
                .WithConferenceStatus(ConferenceState.Closed)
                .Build();
            _newConferenceId5 = conference5.Id;

            var conference6 = new ConferenceBuilder(true)
                .WithParticipant(UserRole.Representative, "Defendant")
                .WithParticipant(UserRole.Judge, null)
                .WithParticipantTask("Test Task")
                .Build();
            _newConferenceId6 = conference6.Id;

            await TestDataManager.SeedConference(conference1);
            await TestDataManager.SeedConference(conference2);
            await TestDataManager.SeedConference(conference3);
            await TestDataManager.SeedConference(conference4);
            await TestDataManager.SeedConference(conference5);
            await TestDataManager.SeedConference(conference6);

            var expectedConferences = new List<Conference> {conference1, conference2, conference3, conference4};
            var conferences = await _handler.Handle(new GetConferencesForTodayByUsernameQuery(username));

            conferences.Should().NotBeEmpty();
            conferences.Select(x => x.Id).Should().BeEquivalentTo(expectedConferences.Select(x => x.Id));
        }

        [TearDown]
        public async Task TearDown()
        {
            TestContext.WriteLine("Cleaning conferences for GetConferencesByUsernameQueryTests");
            await TestDataManager.RemoveConference(_newConferenceId1);
            await TestDataManager.RemoveConference(_newConferenceId2);
            await TestDataManager.RemoveConference(_newConferenceId3);
            await TestDataManager.RemoveConference(_newConferenceId4);
            await TestDataManager.RemoveConference(_newConferenceId5);
            await TestDataManager.RemoveConference(_newConferenceId6);
        }
    }
}