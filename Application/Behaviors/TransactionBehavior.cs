using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IApplicationDbContext context,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply transaction to Commands, not Queries
        if (request is IQuery<TResponse>)
        {
            return await next();
        }

        var response = default(TResponse);
        
        // Cast to DbContext to access Database property
        if (_context is not DbContext dbContext)
            return await next();

        // Check if there's already an active transaction (for nested MediatR calls)
        var currentTransaction = dbContext.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            // Reuse existing transaction
            _logger.LogDebug("Reusing existing transaction for {RequestName}", typeof(TRequest).Name);
            response = await next();
            await _context.SaveChangesAsync(cancellationToken);
            return response!;
        }

        // Create new transaction
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Begin transaction for {RequestName}", typeof(TRequest).Name);

                response = await next();

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Transaction committed for {RequestName}", typeof(TRequest).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during transaction for {RequestName}", typeof(TRequest).Name);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });

        return response!;
    }
}

