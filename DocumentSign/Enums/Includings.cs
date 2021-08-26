using System;

namespace DocumentSign.Enums
{
    [Flags]
    public enum Includings
    {
        none = 0,
        custom_fields = 1,
        documents = 2,
        attachments = 4,
        extensions = 8,
        folders = 16,
        recipients = 32,
        powerform = 64,
        payment_tab = 128
    }
}
