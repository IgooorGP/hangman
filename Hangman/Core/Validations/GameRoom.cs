using FluentValidation;
using Hangman.Core.DTOs;

namespace Hangman.Core.Validations
{
    /// <summary>
    /// Validator for new room creations DTOs.
    /// </summary>
    public class CreateGameRoomDTOValidator : AbstractValidator<CreateGameRoomDTO>
    {
        public CreateGameRoomDTOValidator()
        {
            RuleFor(dto => dto.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("[a-zA-Z]")
            .WithMessage("Game room name must be a string with less than 100 chars (no numbers).");
        }
    }
}