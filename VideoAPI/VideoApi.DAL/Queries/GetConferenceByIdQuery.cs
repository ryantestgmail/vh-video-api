using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoApi.Domain;

namespace VideoApi.DAL.Queries
{
    public class GetConferenceByIdQuery : IQuery
    {
        public Guid HearingId { get; set; }

        public GetConferenceByIdQuery(Guid hearingId)
        {
            HearingId = hearingId;
        }
    }
    
    public class GetConferenceByIdQueryHandler : IQueryHandler<GetConferenceByIdQuery, Conference>
    {
        private readonly VideoApiDbContext _context;

        public GetConferenceByIdQueryHandler(VideoApiDbContext context)
        {
            _context = context;
        }

        public async Task<Conference> Handle(GetConferenceByIdQuery query)
        {
            return await _context.Conferences
                .Include(x => x.Participants).ThenInclude(x => x.ParticipantStatuses)
                .Include(x => x.ConferenceStatuses).AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == query.HearingId);
        }
    }
}