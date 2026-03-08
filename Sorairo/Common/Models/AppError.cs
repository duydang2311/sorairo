using Sorairo.Common.Interfaces;

namespace Sorairo.Common.Models;

public abstract record AppError(string Message) : IAppError;
