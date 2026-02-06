using Application.Common.Interfaces;
using MediatR;

namespace API.Tests.Helpers;

public class TestCommandSaveChangesBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IApplicationDbContext _context;

    public TestCommandSaveChangesBehavior(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();
        await _context.SaveChangesAsync(cancellationToken);
        return response;
    }
}
