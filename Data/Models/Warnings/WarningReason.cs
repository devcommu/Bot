using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevCommuBot.Data.Models.Warnings
{
    internal enum WarningReason : int
    {
        NO_REASON,
        TOS, //Break of discord's TOS
        REPEAT, //Bad users!

    }
}
