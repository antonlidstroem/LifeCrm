using FluentValidation;
using MediatR;
using LifeCrm.Application.Common.Exceptions;
using CustomValidationException = LifeCrm.Application.Common.Exceptions.ValidationException;

namespace LifeCrm.Application.Common.Behaviours
{
    public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators) { _validators = validators; }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return await next();
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(e => e != null)
                .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
            if (failures.Count != 0) throw new CustomValidationException(failures);
            return await next();
        }
    }
}
