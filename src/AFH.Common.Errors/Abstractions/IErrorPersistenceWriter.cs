using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Abstractions;

public interface IErrorPersistenceWriter
{
    Task WriteAsync(ErrorRecord record, CancellationToken cancellationToken = default);
}
