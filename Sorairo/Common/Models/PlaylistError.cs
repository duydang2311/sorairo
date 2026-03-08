namespace Sorairo.Common.Models;

public sealed record PlaylistError(PlaylistErrorKind Kind, string Message) : AppError(Message);
