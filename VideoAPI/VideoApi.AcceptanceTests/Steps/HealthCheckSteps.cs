﻿using System;
using TechTalk.SpecFlow;
using Testing.Common.Helper;
using VideoApi.AcceptanceTests.Contexts;

namespace VideoApi.AcceptanceTests.Steps
{
    [Binding]
    public sealed class HealthCheckSteps : BaseSteps
    {
        private readonly TestContext _context;
        private readonly HealthCheckEndpoints _endpoints = new ApiUriFactory().HealthCheckEndpoints;

        public HealthCheckSteps(TestContext injectedContext)
        {
            _context = injectedContext;
        }

        [Given(@"I have a get health request")]
        public void GivenIHaveAGetHealthRequest()
        {
            _context.NewConferenceId = Guid.Empty;
            _context.Request = _context.Get(_endpoints.CheckServiceHealth());
        }
    }
}
