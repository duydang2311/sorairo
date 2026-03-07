using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Sorairo.Common.Messaging;

public sealed class OpenSingleFileDialogMessage : AsyncRequestMessage<Uri?>;
