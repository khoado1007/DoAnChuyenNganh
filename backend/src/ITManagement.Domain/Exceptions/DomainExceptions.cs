namespace ITManagement.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} với id '{id}' không tồn tại.") { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Bạn không có quyền thực hiện thao tác này.")
        : base(message) { }
}

public class QuotaExceededException : DomainException
{
    public QuotaExceededException(long fileSizeBytes, long availableBytes)
        : base($"Dung lượng tệp ({fileSizeBytes:N0} bytes) vượt quá hạn mức còn lại ({availableBytes:N0} bytes).") { }
}

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Dữ liệu không hợp lệ. Kiểm tra các trường bị lỗi.")
    {
        Errors = errors.AsReadOnly();
    }
}

public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}
