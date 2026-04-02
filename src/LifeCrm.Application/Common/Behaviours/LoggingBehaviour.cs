using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Common.Behaviours
{
    public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger) { _logger = logger; }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var name = typeof(TRequest).Name;
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[MediatR] Handling {RequestName}", name);
            try
            {
                var response = await next();
                sw.Stop();
                if (sw.ElapsedMilliseconds > 500)
                    _logger.LogWarning("[MediatR] Slow request: {RequestName} took {Ms}ms", name, sw.ElapsedMilliseconds);
                else
                    _logger.LogInformation("[MediatR] Handled {RequestName} in {Ms}ms", name, sw.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[MediatR] Error handling {RequestName} after {Ms}ms", name, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
