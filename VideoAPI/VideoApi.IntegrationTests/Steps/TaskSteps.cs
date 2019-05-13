using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Testing.Common.Helper;
using Testing.Common.Helper.Builders.Domain;
using VideoApi.Contract.Requests;
using VideoApi.Contract.Responses;
using VideoApi.Domain;
using VideoApi.Domain.Enums;
using VideoApi.IntegrationTests.Contexts;
using VideoApi.IntegrationTests.Helper;
using Task = VideoApi.Domain.Task;

namespace VideoApi.IntegrationTests.Steps
{
    [Binding]
    public class TaskSteps : StepsBase
    {
        private readonly TaskEndpoints _endpoints = new ApiUriFactory().TaskEndpoints;
        
        public TaskSteps(ApiTestContext apiTestContext) : base(apiTestContext)
        {
        }

        [Given(@"I have a (.*) get pending tasks request")]
        [Given(@"I have an (.*) get pending tasks request")]
        public async System.Threading.Tasks.Task GivenIHaveAGetPendingTasksRequest(Scenario scenario)
        {
            Guid conferenceId;
            switch (scenario)
            {
                case Scenario.Valid:
                    var seededConference = await SeedConferenceWithTasks();
                    TestContext.WriteLine($"New seeded conference id: {seededConference.Id}");
                    ApiTestContext.NewConferenceId = seededConference.Id;
                    conferenceId = seededConference.Id;
                    break;
                case Scenario.Nonexistent:
                    conferenceId = Guid.NewGuid();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
            }
            
            ApiTestContext.Uri = _endpoints.GetPendingTasks(conferenceId);
            ApiTestContext.HttpMethod = HttpMethod.Get;
        }

        [Then(@"the list of tasks should be retrieved")]
        public async System.Threading.Tasks.Task ThenTheListOfTasksShouldBeRetrieved()
        {
            var json = await ApiTestContext.ResponseMessage.Content.ReadAsStringAsync();
            var tasks = ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<TaskResponse>>(json);
            tasks.Should().NotBeNullOrEmpty();
            foreach (var task in tasks)
            {
                task.Id.Should().BeGreaterThan(0);
                task.Body.Should().NotBeNullOrWhiteSpace();
                task.Type.Should().BeOfType<TaskType>();
            }
        }

        [Given(@"I have a (.*) update task request")]
        [Given(@"I have an (.*) update task request")]
        public async System.Threading.Tasks.Task GivenIHaveAUpdateTaskRequest(Scenario scenario)
        {
            var seededConference = await SeedConferenceWithTasks();
            TestContext.WriteLine($"New seeded conference id: {seededConference.Id}");
            ApiTestContext.NewConferenceId = seededConference.Id;

            var conferenceId = seededConference.Id;
            long taskId;
            var request = new UpdateTaskRequest
            {
                UpdatedBy = seededConference.Participants
                    .First(x => x.UserRole == UserRole.Individual).Username
            };

            switch (scenario)
            {
                case Scenario.Valid:
                    var task = seededConference.Tasks.First(x => x.Type == TaskType.Participant);
                    taskId = task.Id;
                    break;
                case Scenario.Invalid:
                    taskId = 0;
                    break;
                case Scenario.Nonexistent:
                    taskId = 111111;
                    conferenceId = Guid.NewGuid();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
            }

            ApiTestContext.Uri = _endpoints.UpdateTaskStatus(conferenceId, taskId);
            ApiTestContext.HttpMethod = HttpMethod.Patch;
            var jsonBody = ApiRequestHelper.SerialiseRequestToSnakeCaseJson(request);
            ApiTestContext.HttpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        private async Task<Conference> SeedConferenceWithTasks()
        {
            const string body = "Automated Test Complete Task";
            const string updatedBy = "test@automated.com";

            var judgeTaskDone = new Task(body, TaskType.Judge);
            judgeTaskDone.CompleteTask(updatedBy);
            var participantTaskDone = new Task(body, TaskType.Participant);
            participantTaskDone.CompleteTask(updatedBy);
            var hearingTaskDone = new Task(body, TaskType.Hearing);
            hearingTaskDone.CompleteTask(updatedBy);
            
            var conference = new ConferenceBuilder(true)
                .WithParticipant(UserRole.Individual, "Claimant")
                .Build();
            
            conference.AddTask(TaskType.Judge, body);
            conference.AddTask(TaskType.Participant, body);
            conference.AddTask(TaskType.Hearing, body);
            conference.AddTask(TaskType.Participant, body);

            conference.GetTasks()[0].CompleteTask(updatedBy);
            
            return await ApiTestContext.TestDataManager.SeedConference(conference);
        }
        
    }
}