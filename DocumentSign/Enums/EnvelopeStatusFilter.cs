using System;

namespace DocumentSign.Enums
{
    [Flags]
    public enum EnvelopeStatusFilter
    {
        any = 0,
        created = 1,
        deleted = 2,
        sent = 4,
        delivered = 8,
        signed = 16,
        completed = 32,
        declined = 64,
        timedout = 128,
        voided = 256
    }
}

