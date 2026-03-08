using Sorairo.Common.Interfaces;

namespace Sorairo.Common.Models;

public readonly struct NotFoundError : IAppError
{
    public string Message => "Not found";
}
