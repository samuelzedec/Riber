using Riber.Domain.Abstractions;
using Riber.Domain.Constants;
using Riber.Domain.Validators.DocumentValidator.Exceptions;

namespace Riber.Domain.Validators.DocumentValidator;

public sealed record CpfValidator : IDocumentValidator
{
    #region Properties Private

    private const int ExpectedLength = 11;

    #endregion

    #region Methods

    public void IsValid(string document)
    {
        string cpf = Sanitize(document);
            
        if (cpf.Distinct().Count() == 1)
            throw new InvalidCpfException(ErrorMessage.Cpf.OnlyRepeatedDigits);
            
        int[] digits = cpf
            .Select(c => c - '0')
            .ToArray();
        
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += digits[i] * (10 - i);
        
        int remainder = sum % 11;
        int digit1 = remainder < 2 ? 0 : 11 - remainder;
        
        if (digits[9] != digit1)
            throw new InvalidCpfException(ErrorMessage.Cpf.IsInvalid);
            
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += digits[i] * (11 - i);
        
        remainder = sum % 11;
        int digit2 = remainder < 2 ? 0 : 11 - remainder;

        if (digits[10] != digit2)
            throw new InvalidCpfException(ErrorMessage.Cpf.IsInvalid);
    }

    public string Sanitize(string document)
    {
        if (string.IsNullOrWhiteSpace(document))
            throw new InvalidCpfException(ErrorMessage.Cpf.IsNullOrEmpty);
        
        var sanitized = IDocumentValidator.SanitizeStatic(document);
        return IDocumentValidator.ValidateLength(
            sanitized, 
            ExpectedLength, 
            new InvalidLengthCpfException(ErrorMessage.Cpf.LengthIsInvalid)
        );
    }

    public static string Format(string document)
        => $"{document.Substring(0, 3)}.{document.Substring(3, 3)}.{document.Substring(6, 3)}-{document.Substring(9, 2)}";

    #endregion
}