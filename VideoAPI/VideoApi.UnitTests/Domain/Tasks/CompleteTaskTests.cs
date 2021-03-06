using System;
using FluentAssertions;
using NUnit.Framework;
using VideoApi.Domain;
using VideoApi.Domain.Enums;

namespace VideoApi.UnitTests.Domain.Tasks
{
    public class CompleteTaskTests
    {
        [Test]
        public void should_not_be_completed_by_default()
        {
            var alert = new Task(Guid.NewGuid(), "Something happened", TaskType.Participant);
            alert.Status.Should().Be(TaskStatus.ToDo);
            alert.Updated.Should().BeNull();
        }

        [Test]
        public void should_update_status_to_done()
        {
            var alert = new Task(Guid.NewGuid(), "Something happened", TaskType.Participant);
            const string user = "Test User";

            alert.CompleteTask(user);
            alert.Status.Should().Be(TaskStatus.Done);
            alert.UpdatedBy.Should().Be(user);
            alert.Updated.Should().NotBeNull();
        }
    }
}