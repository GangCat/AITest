using System;
using System.Collections.Generic;
using System.Text;

namespace AITest
{
    internal class PlayerPolicy
    {
        public static EAction DecideNextAction(float pHP, float eHP, EAction pLA, EAction eLA)
        {
            float pA = 0.33f;
            float pC = 0.33f;
            float pH = 0.33f;

            if (pHP < 0.3f)
            {
                pH += 0.30f;
                pA -= 0.15f;
                pC -= 0.15f;
            }

            if (eLA == EAction.HEAL)
                pA += 0.25f;

            if (eLA == EAction.ATTACK)
                pC += 0.20f;

            if (eLA == EAction.COUNTER)
                pH += 0.20f;

            if (eHP < 0.25f)
                pA += 0.30f;

            pA = Math.Max(pA, 0f);
            pC = Math.Max(pC, 0f);
            pH = Math.Max(pH, 0f);

            float sum = pA + pC + pH;
            pA /= sum;
            pC /= sum;
            pH /= sum;

            var random = new Random();

            float r = random.NextSingle();

            if (r < pA) return EAction.ATTACK;
            if (r < pA + pC) return EAction.COUNTER;
            return EAction.HEAL;
        }
    }
}
