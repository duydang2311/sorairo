using ATL;

namespace Sorairo.Common.Models;

public sealed record PlaylistItem
{
    public required Guid Id { get; init; }
    public required Uri Path { get; init; }
    public string? Artist { get; init; }
    public string? Title { get; init; }
    public string? Album { get; init; }

    public byte[]? GetFrontCover()
    {
        var track = new Track(Path.LocalPath);
        return track
            .EmbeddedPictures.FirstOrDefault(a => a.PicType == PictureInfo.PIC_TYPE.Front)
            ?.PictureData;
    }
}
