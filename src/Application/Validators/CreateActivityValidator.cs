using Application.Activities.Commands;
using FluentValidation;

namespace Application.Validators
{
    public sealed class CreateActivityValidator : AbstractValidator<CreateActivityCommand>
    {
        public CreateActivityValidator()
        {
            RuleFor(x => x.DurationMin).GreaterThan(0);
            RuleFor(x => x.StartAtUtc).NotEmpty();
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.PerceivedExertion).InclusiveBetween(1, 10).When(x => x.PerceivedExertion.HasValue);
            RuleFor(x => x.DistanceKm).GreaterThanOrEqualTo(0).When(x => x.DistanceKm.HasValue);
            RuleFor(x => x.Steps).GreaterThanOrEqualTo(0).When(x => x.Steps.HasValue);
            RuleFor(x => x.Calories).GreaterThanOrEqualTo(0).When(x => x.Calories.HasValue);
        }
    }
}
