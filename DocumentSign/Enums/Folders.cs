using System;

namespace DocumentSign.Enums
{
    [Flags]
    public enum Folders : int
    {
        all = 0,
        awaiting_my_signature = 1,
        completed = 2,
        draft = 4,
        expiring_soon = 8,
        drafts = 16,
        inbox = 32,
        out_for_signature = 64,
        recyclebin = 128,
        sentitems = 256,
        waiting_for_others = 512,
    }
}
