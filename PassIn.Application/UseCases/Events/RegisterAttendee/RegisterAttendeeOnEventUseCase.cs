using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;

namespace PassIn.Application.UseCases.Events.RegisterAttendee
{
    public class RegisterAttendeeOnEventUseCase
    {
        private readonly PassInDbContext _dbContext;
        public RegisterAttendeeOnEventUseCase()
        {
            _dbContext = new PassInDbContext();
        }

        public ResponseRegisteredJson Execute(Guid eventId, RequestRegisterEventJson request)
        {
            Validate(request, eventId);

            var entity = new Infrastructure.Entities.Attendee
            {
                Email = request.Email,
                Name = request.Name,
                Event_Id = eventId,
                Created_At = DateTime.UtcNow,
            };

            _dbContext.Attendees.Add(entity);
            _dbContext.SaveChanges();

            return new ResponseRegisteredJson
            {
                Id = entity.Id,
            };
            
        }

        private void Validate(RequestRegisterEventJson request, Guid eventId)
        {
            var eventEntity = _dbContext.Events.Find(eventId) ?? throw new ConflictException("An event with this id does not exist.");

            var attendeesCount = _dbContext.Attendees.Count(attendee => attendee.Event_Id == eventId);

            if(attendeesCount >= eventEntity.Maximum_Attendees)
                throw new ErrorOnValidationException("There is no more room for this event.");

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ErrorOnValidationException("The name is invalid.");
            }
            if (ValidateEmail(request.Email) == false)
            {
                throw new ErrorOnValidationException("The Email is invalid.");
            }

            var isAttendeeAlreadyRegistered = _dbContext.Attendees
                .Any(attendee => attendee.Email.Equals(request.Email) && attendee.Event_Id == eventId);

            if(isAttendeeAlreadyRegistered )
            {
                throw new ConflictException("You've already registered to this event.");
            }

        }

        private bool ValidateEmail(string email)
        {
            try
            {
                var generatedEmail = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
