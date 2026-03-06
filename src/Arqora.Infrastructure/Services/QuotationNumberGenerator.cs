using Arqora.Application.Interfaces;
using Arqora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arqora.Infrastructure.Services;

/// <summary>
/// Generates sequential, human-readable quotation numbers in the format:
///   ARQ-{YYYY}-{0001}
///
/// The generator queries the database for the highest number used in the
/// current year and increments it. This approach is simple and works well
/// for low-to-moderate concurrency. In a high-concurrency scenario you'd
/// want a database sequence or a distributed ID generator to avoid race
/// conditions between concurrent SaveChanges calls.
/// </summary>
public class QuotationNumberGenerator : IQuotationNumberGenerator
{
    private readonly ArqoraDbContext _context;

    public QuotationNumberGenerator(ArqoraDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"ARQ-{currentYear}-";

        // Find the highest existing number for this year.
        // We filter by prefix and then parse the trailing digits.
        var lastNumber = await _context.Quotations
            .Where(q => q.QuotationNumber.StartsWith(prefix))
            .OrderByDescending(q => q.QuotationNumber)
            .Select(q => q.QuotationNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;

        if (lastNumber is not null)
        {
            // Extract the 4-digit sequence from "ARQ-2026-0042" → "0042" → 42
            var sequencePart = lastNumber[prefix.Length..];
            if (int.TryParse(sequencePart, out var parsed))
                nextSequence = parsed + 1;
        }

        return $"{prefix}{nextSequence:D4}";
    }
}
