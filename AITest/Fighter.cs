using System;
using System.Collections.Generic;
using System.Text;

namespace AITest
{
    internal class Fighter
    {
        public float HP = 100f;
        public EAction LastAction = EAction.NONE; // Attack=0, Counter=1, Heal=2
    }

    internal enum  EAction
    {
        NONE = -1,
        ATTACK,
        COUNTER,
        HEAL
    }
}
