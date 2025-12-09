using System;

namespace AITest
{
    internal class PlayerPolicy
    {
        private static readonly Random random = new Random();

        public static EAction DecideNextAction(float pHP, float eHP, EAction pLA, EAction eLA)
        {
            float pA = 0.33f;
            float pC = 0.33f;
            float pH = 0.33f;

            // -------------------------------------------
            // 1) HP 기반 판단 (부드러운 판단)
            // -------------------------------------------
            // HP가 낮을수록 회복 확률 점진적 증가
            pH += (0.4f - pHP); // pHP=1.0일때  -0.6 / pHP=0.1일때 +0.3

            // HP가 매우 높으면 공격 성향 강화
            pA += Math.Max(0f, pHP - 0.7f) * 0.4f;

            // HP가 중간이면 반격도 고려
            pC += (0.5f - Math.Abs(pHP - 0.5f)) * 0.1f;

            // -------------------------------------------
            // 2) Enemy의 마지막 행동 기반 (사람은 전략이 흔들림)
            // -------------------------------------------
            if (eLA == EAction.HEAL)
            {
                pA += 0.15f + random.NextSingle() * 0.15f; // 랜덤 가중치
            }
            else if (eLA == EAction.ATTACK)
            {
                pC += 0.10f + random.NextSingle() * 0.15f;
            }
            else if (eLA == EAction.COUNTER)
            {
                pH += 0.10f + random.NextSingle() * 0.15f;
            }

            // -------------------------------------------
            // 3) Enemy HP 기반 판단 (마무리 성향)
            // -------------------------------------------
            if (eHP < 0.35f)
            {
                pA += 0.25f + random.NextSingle() * 0.15f;
            }

            // -------------------------------------------
            // 4) Player의 지난 행동을 반복하려는 경향 (사람 특징)
            // -------------------------------------------
            if (pLA == EAction.ATTACK) pA += 0.10f;
            if (pLA == EAction.COUNTER) pC += 0.10f;
            if (pLA == EAction.HEAL) pH += 0.15f;

            // -------------------------------------------
            // 5) 노이즈 추가 (AI가 너무 예측 가능해지지 않도록)
            // -------------------------------------------
            pA += (random.NextSingle() - 0.5f) * 0.10f;
            pC += (random.NextSingle() - 0.5f) * 0.10f;
            pH += (random.NextSingle() - 0.5f) * 0.10f;

            // -------------------------------------------
            // 6) 정규화
            // -------------------------------------------
            pA = Math.Max(pA, 0f);
            pC = Math.Max(pC, 0f);
            pH = Math.Max(pH, 0f);

            float sum = pA + pC + pH;
            pA /= sum;
            pC /= sum;
            pH /= sum;

            // -------------------------------------------
            // 7) 확률 선택
            // -------------------------------------------
            float r = random.NextSingle();

            if (r < pA) return EAction.ATTACK;
            if (r < pA + pC) return EAction.COUNTER;
            return EAction.HEAL;
        }
    }
}
