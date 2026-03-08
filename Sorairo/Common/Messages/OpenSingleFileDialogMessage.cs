using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Sorairo.Common.Messages;

public sealed class OpenSingleFileDialogMessage : AsyncRequestMessage<Uri?>;
