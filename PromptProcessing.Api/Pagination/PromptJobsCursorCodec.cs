using System.Buffers.Binary;
using Microsoft.AspNetCore.WebUtilities;

namespace PromptProcessing.Api.Pagination;

public sealed class PromptJobsCursorCodec
{
    private const int CursorLength = sizeof(long) + 16;

    public string? Encode(PromptJobsCursor? cursor)
    {
        if (cursor is null)
            return null;

        Span<byte> bytes = stackalloc byte[CursorLength];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, cursor.CreatedAtUtc.Ticks);
        cursor.Id.TryWriteBytes(bytes[sizeof(long)..]);

        return WebEncoders.Base64UrlEncode(bytes);
    }

    public PromptJobsCursor? Decode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            var bytes = WebEncoders.Base64UrlDecode(value);

            if (bytes.Length != CursorLength)
                throw new InvalidPromptJobsCursorException();

            var ticks = BinaryPrimitives.ReadInt64LittleEndian(bytes);
            var createdAtUtc = new DateTime(ticks, DateTimeKind.Utc);
            var id = new Guid(bytes.AsSpan(sizeof(long)));

            return new PromptJobsCursor(createdAtUtc, id);
        }
        catch (Exception exception) when (exception is FormatException or ArgumentOutOfRangeException)
        {
            throw new InvalidPromptJobsCursorException(exception);
        }
    }
}