using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Video.API.Validations;
using VideoApi.Contract.Requests;

namespace VideoApi.UnitTests.Validation
{
    public class LeaveConsultationRequestValidationTests
    {
        private LeaveConsultationRequestValidation _validator;
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _validator = new LeaveConsultationRequestValidation();
        }
        
        [Test]
        public async Task should_pass_validation()
        {
            var request = new LeaveConsultationRequest
            {
                ConferenceId = Guid.NewGuid(),
                ParticipantId = Guid.NewGuid()
            };

            var result = await _validator.ValidateAsync(request);

            result.IsValid.Should().BeTrue();
        }
        
        [Test]
        public async Task should_return_missing_conference_id()
        {
            var request = new LeaveConsultationRequest
            {
                ParticipantId = Guid.NewGuid()
            };

            var result = await _validator.ValidateAsync(request);

            result.IsValid.Should().BeFalse();
            result.Errors
                .Any(x => x.ErrorMessage == LeaveConsultationRequestValidation.NoConferenceIdErrorMessage)
                .Should().BeTrue();
        }
        
        [Test]
        public async Task should_return_missing_participant_id()
        {
            var request = new LeaveConsultationRequest
            {
                ConferenceId = Guid.NewGuid()
            };

            var result = await _validator.ValidateAsync(request);

            result.IsValid.Should().BeFalse();
            result.Errors
                .Any(x => x.ErrorMessage == LeaveConsultationRequestValidation.NoParticipantIdErrorMessage)
                .Should().BeTrue();
        }
        
    }
}