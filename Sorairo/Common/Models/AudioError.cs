namespace Sorairo.Common.Models;

public sealed record AudioError(AudioErrorKind Kind, string Message) : AppError(Message);
