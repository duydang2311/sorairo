using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Sorairo.Common.Messages;

public sealed class OpenMultiFilesDialogMessage : AsyncRequestMessage<List<Uri>>;
