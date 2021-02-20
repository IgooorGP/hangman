using FluentValidation;
using Hangman.Core.DTOs;
using Hangman.Core.Services;

namespace Hangman.Core.Validations
{
    public class CreateGameRoomDTOValidator : AbstractValidator<CreateGameRoomDTO>
    {
        public CreateGameRoomDTOValidator(IGameRoomSvc gameRoomSvc)
        {
            RuleFor(dto => dto.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("[a-zA-Z]")
            .WithMessage("Game room name must be a string with less than 100 chars (no numbers).");
        }
    }
}