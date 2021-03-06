﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using TechTalk.SpecFlow;
using Testing.Common.Assertions;
using Testing.Common.Helper;
using Testing.Common.Helper.Builders.Api;
using VideoApi.AcceptanceTests.Contexts;
using VideoApi.Common.Helpers;
using VideoApi.Contract.Requests;
using VideoApi.Contract.Responses;
using VideoApi.Domain.Enums;

namespace VideoApi.AcceptanceTests.Steps
{
    [Binding]
    public sealed class ConferenceSteps : BaseSteps
    {
        private readonly TestContext _context;
        private readonly ScenarioContext _scenarioContext;
        private readonly ConferenceEndpoints _endpoints = new ApiUriFactory().ConferenceEndpoints;
        private const string CurrentStatusKey = "CurrentStatus";
        private const string UpdatedKey = "UpdatedConference";

        public ConferenceSteps(TestContext injectedContext, ScenarioContext scenarioContext)
        {
            _context = injectedContext;
            _scenarioContext = scenarioContext;
        }

        [Given(@"I have a get details for a conference request by username with a valid username")]
        public void GivenIHaveAGetDetailsForAConferenceRequestByUsernameWithAValidUsername()
        {
            _context.SetDefaultBearerToken();
            _context.Request = _context.Get(_endpoints.GetConferenceDetailsByUsername(_context.NewConference.Participants.First().Username));
        }

        [Given(@"I have an update conference request")]
        public void GivenIHaveAnUpdateConferenceRequest()
        {

            var request = new UpdateConferenceRequest
            {
                CaseName = $"{_context.NewConference.CaseName} UPDATED",
                CaseNumber = $"{_context.NewConference.CaseNumber} UPDATED",
                CaseType = "Financial Remedy",
                HearingRefId = _context.NewHearingRefId,
                ScheduledDateTime = DateTime.Now.AddDays(1),
                ScheduledDuration = 12,
                HearingVenueName = "MyVenue"
            };

            _scenarioContext.Add(UpdatedKey, request);
            _context.Request = _context.Put(_endpoints.UpdateConference, request);
        }

        [Given(@"I have a valid book a new conference request")]
        public void GivenIHaveAValidBookANewConferenceRequest()
        {
            CreateNewConferenceRequest(DateTime.Now.ToLocalTime().AddMinutes(2));
        }

        [Given(@"I have a conference")]
        public void GivenIHaveAConference()
        {
            CreateConference(DateTime.Now.ToLocalTime().AddMinutes(2));
        }

        [Given(@"I have another conference")]
        public void GivenIHaveAnotherConference()
        {
            _context.NewConferenceIds.Add(_context.NewConferenceId);
            CreateConference(DateTime.Now);
            _context.NewConferenceIds.Add(_context.NewConferenceId);
        }
        
        [Given(@"I close the last created conference")]
        public void GivenICloseTheLastCreatedConference()
        {
            var conferenceId = _context.NewConferenceIds.LastOrDefault();
            
            if (conferenceId == Guid.Empty)
            {
                throw new Exception("Could not get id of the last conference created");
            }
            
            CloseAndCheckConferenceClosed(conferenceId);
        }
        
        [Given(@"I close all conferences")]
        public void GivenICloseAllConferences()
        {
            _context.NewConferenceIds.ForEach(x =>
            {
                if (x == Guid.Empty)
                {
                    throw new Exception("Could not get id of the conference created");
                }
            
                CloseAndCheckConferenceClosed(x);
            });
        }

        [Given(@"I have a conference for tomorrow")]
        public void GivenIHaveAConferenceForTomorrow()
        {
            CreateConference(DateTime.Today.AddDays(1));
            _context.NewConferenceIds.Add(_context.NewConferenceId);
        }
        
        [Given(@"I have a conference for yesterday")]
        public void GivenIHaveAConferenceForYesterday()
        {
            CreateConference(DateTime.Today.AddDays(-1));
            _context.NewConferenceIds.Add(_context.NewConferenceId);
        }

        [Given(@"I have a get details for a conference request with a valid conference id")]
        public void GivenIHaveAGetDetailsForAConferenceRequestWithAValidConferenceId()
        {
            _context.Request = _context.Get(_endpoints.GetConferenceDetailsById(_context.NewConferenceId));
        }

        [Given(@"I have a valid delete conference request")]
        public void GivenIHaveAValidDeleteConferenceRequest()
        {
            _context.Request = _context.Delete(_endpoints.RemoveConference(_context.NewConferenceId));
        }

        [Given(@"I have a get conferences for today request with a valid date")]
        public void GivenIHaveAValidGetTodaysConferencesRequest()
        {
            _context.Request = _context.Get(_endpoints.GetConferencesToday);
        }

        [Given(@"I have a get expired conferences request")]
        public void GivenIHaveAGetExpiredConferencesRequest()
        {
            _context.Request = _context.Get(_endpoints.GetExpiredOpenConferences);
        }

        [Given(@"I have a get details for a conference request by hearing id with a valid username")]
        public void GivenIHaveAGetDetailsForAConferenceRequestByHearingIdWithAValidUsername()
        {
            _context.Request = _context.Get(_endpoints.GetConferenceByHearingRefId(_context.NewHearingRefId));
        }

        [Then(@"the conference details have been updated")]
        public void ThenICanSeeTheConferenceDetailsHaveBeenUpdated()
        {
            _context.Request = _context.Get(_endpoints.GetConferenceDetailsById(_context.NewConferenceId));
            _context.Response = _context.Client().Execute(_context.Request);
            _context.Response.IsSuccessful.Should().BeTrue("Conference details are retrieved");
            var conference = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<ConferenceDetailsResponse>(_context.Response.Content);
            conference.Should().NotBeNull();

            var expected = _scenarioContext.Get<UpdateConferenceRequest>(UpdatedKey);
            conference.CaseName.Should().Be(expected.CaseName);
            conference.CaseNumber.Should().Be(expected.CaseNumber);
            conference.CaseType.Should().Be(expected.CaseType);
            conference.ScheduledDateTime.Day.Should().Be(DateTime.Today.AddDays(1).Day);
            conference.ScheduledDuration.Should().Be(expected.ScheduledDuration);
            conference.HearingVenueName.Should().Be(expected.HearingVenueName);
        }

        [Then(@"the conference details should be retrieved")]
        public void ThenTheConferenceDetailsShouldBeRetrieved()
        {
            var conference = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<ConferenceDetailsResponse>(_context.Json);
            conference.Should().NotBeNull();
            _context.NewConferenceId = conference.Id;
            AssertConferenceDetailsResponse.ForConference(conference);
        }

        [Then(@"a list containing only todays hearings conference details should be retrieved")]
        public void ThenAListOfTheConferenceDetailsShouldBeRetrieved()
        {
            var conferences = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ConferenceSummaryResponse>>(_context.Json);
            conferences.Should().NotBeNull();
            foreach (var conference in conferences)
            {
                AssertConferenceSummaryResponse.ForConference(conference);
                foreach (var participant in conference.Participants)
                {
                    AssertParticipantSummaryResponse.ForParticipant(participant);
                }
                conference.ScheduledDateTime.DayOfYear.Should().Be(DateTime.Now.DayOfYear);
            }

            _context.NewConferences = conferences.Where(x => x.CaseName.StartsWith("Automated Test Hearing")).ToList();
        }

        [Then(@"I have an empty list of expired conferences")]
        public void ThenAListOfNonClosedConferenceDetailsShouldBeRetrieved()
        {
            var conferences = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ExpiredConferencesResponse>>(_context.Json);
            conferences.Should().NotContain(x => x.CurrentStatus == ConferenceState.Closed);
        }
        
        [Then(@"a list not containing the closed hearings should be retrieved")]
        public void ThenAListNotContainingTheClosedHearingsShouldBeRetrieved()
        {
            var conferences = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ExpiredConferencesResponse>>(_context.Json);
            conferences.Select(x => x.Id).Should().NotContain(_context.NewConferenceIds);
        }

        [Then(@"the summary of conference details should be retrieved")]
        public void ThenTheSummaryOfConferenceDetailsShouldBeRetrieved()
        {
            var conferences = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ConferenceSummaryResponse>>(_context.Json);
            conferences.Should().NotBeNull();
            _context.NewConferenceId = conferences.First().Id;
            foreach (var conference in conferences)
            {
                AssertConferenceSummaryResponse.ForConference(conference);
                foreach (var participant in conference.Participants)
                {
                    AssertParticipantSummaryResponse.ForParticipant(participant);
                }
            }
        }

        [Then(@"the conference should be removed")]
        public void ThenTheConferenceShouldBeRemoved()
        {
            _context.Request = _context.Get(_endpoints.GetConferenceDetailsById(_context.NewConferenceId));
            _context.Response = _context.Client().Execute(_context.Request);
            _context.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _context.NewConferenceId = Guid.Empty;
        }

        private void CreateConference(DateTime date)
        {
            CreateNewConferenceRequest(date);
            _context.Response = _context.Client().Execute(_context.Request);
            _context.Response.IsSuccessful.Should().BeTrue("New conference is created");
            var conference =
                ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<ConferenceDetailsResponse>(_context.Response.Content);
            conference.Should().NotBeNull();
            _context.NewConferenceId = conference.Id;
            _context.NewConference = conference;
            if (!_scenarioContext.ContainsKey(CurrentStatusKey))
                _scenarioContext.Add(CurrentStatusKey, conference.CurrentStatus);
        }

        private void UpdateConferenceStateToClosed(Guid conferenceId)
        {
            _context.Request = _context.Put(_endpoints.CloseConference(conferenceId), new object());
            _context.Response = _context.Client().Execute(_context.Request);
            _context.Response.IsSuccessful.Should().BeTrue("Conference is closed");
        }

        private void CreateNewConferenceRequest(DateTime date)
        {
            _context.NewHearingRefId = Guid.NewGuid();
            var request = new BookNewConferenceRequestBuilder()
                .WithJudge()
                .WithRepresentative("Claimant").WithIndividual("Claimant")
                .WithRepresentative("Defendant").WithIndividual("Defendant")
                .WithHearingRefId(_context.NewHearingRefId)
                .WithDate(date)
                .Build();
            _context.Request = _context.Post(_endpoints.BookNewConference, request);
        }

        private void CloseAndCheckConferenceClosed(Guid conferenceId)
        {
            UpdateConferenceStateToClosed(conferenceId);

            _context.Request = _context.Get(_endpoints.GetConferenceDetailsById(conferenceId));
            _context.Response = _context.Client().Execute(_context.Request);
            _context.Response.IsSuccessful.Should().BeTrue("Conference details are retrieved");
            var conference = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<ConferenceDetailsResponse>(_context.Response.Content);
            conference.CurrentStatus.Should().Be(ConferenceState.Closed);
        }
    }
}

