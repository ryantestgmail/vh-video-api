using VideoApi.Domain.Enums;

namespace VideoApi.Contract.Responses
{
    public class ParticipantSummaryResponse
    {
        /// <summary>
        /// The participant username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// The participant display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// The current participant status
        /// </summary>
        public ParticipantState Status { get; set; }
        
        /// <summary>
        /// The participant role in conference
        /// </summary>
        public UserRole UserRole { get; set; }
    }
}