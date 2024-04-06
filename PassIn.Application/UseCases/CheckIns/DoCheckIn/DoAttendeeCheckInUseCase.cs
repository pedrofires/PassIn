using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using PassIn.Infrastructure.Entities;

namespace PassIn.Application.UseCases.CheckIns.DoCheckIn
{
    public class DoAttendeeCheckInUseCase
    {
        private readonly PassInDbContext _dbContext;
        public DoAttendeeCheckInUseCase()
        {
            _dbContext = new PassInDbContext();
        }

        public ResponseRegisteredJson Execute(Guid attendeeId)
        {
            Validate(attendeeId);

            var entity = new CheckIn
            {
                Attendee_Id = attendeeId,
                Created_At = DateTime.UtcNow,
            };

            _dbContext.CheckIns.Add(entity);
            _dbContext.SaveChanges();

            return new ResponseRegisteredJson {
                Id = entity.Id,
            };
        }

        private void Validate(Guid attendeeId)
        {
            var isAttendeeExists = _dbContext.Attendees.Any(attendee => attendee.Id == attendeeId);
            if(isAttendeeExists == false)
            {
                throw new NotFoundException("The attendee with this Id was not founded.");
            }

            var isExistsCheckIn = _dbContext.CheckIns.Any(ch => ch.Attendee_Id == attendeeId);
            if(isExistsCheckIn) {
                throw new ConflictException("You've already checkedIn to this event.");
            }
        }
    }
}
