using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace AITest
{
    internal class BattleMaster
    {
        Fighter player = new Fighter();
        Fighter enemy = new Fighter();
        private readonly HttpClient client = new HttpClient();

        double pAttack, pCounter, pHeal;

        const int MAX_TURNS = 100000;

        int playerStreak = 0;
        int enemyStreak = 0;

        int turn = 1;

        int deltaPlayerHP = 0;
        int deltaEnemyHP = 0;

        public void Start()
        {
            BattleLogger.Initialize(@"D:\AIData\battle_data.csv");

            player.HP = 100;
            enemy.HP = 100;
            player.LastAction = EAction.NONE;
            enemy.LastAction = EAction.NONE;
            var random = new Random();

            for (int i = 0; i < MAX_TURNS; i++)
            {
                EAction nextPlayerAction = PlayerPolicy.DecideNextAction(
                    player.HP / 100f,
                    enemy.HP / 100f,
                    player.LastAction,
                    enemy.LastAction
                );

                var enemyActionIdx = random.NextInt64(0, 3);
                EAction enemyAction = (EAction)enemyActionIdx;

                ResolveTurn(nextPlayerAction, enemyAction);

                // 3) delta 계산을 위해 old HP 저장
                int oldPlayerHP = player.HP;
                int oldEnemyHP = enemy.HP;

                // 4) 전투 계산 (너의 룰 그대로)
                ResolveTurn(nextPlayerAction, enemyAction, ref player.HP, ref enemy.HP);

                // 5) delta 계산
                deltaPlayerHP = CalcDelta(oldPlayerHP, player.HP);
                deltaEnemyHP = CalcDelta(oldEnemyHP, enemy.HP);

                // 6) streak 업데이트
                playerStreak = UpdateStreak(player.LastAction, nextPlayerAction, playerStreak);
                enemyStreak = UpdateStreak(enemy.LastAction, enemyAction, enemyStreak);

                // 7) CSV 기록
                RecordState(
                    player.HP / 100f,
                    enemy.HP / 100f,
                    player.LastAction,
                    enemy.LastAction,
                    turn,
                    playerStreak,
                    enemyStreak,
                    deltaPlayerHP,
                    deltaEnemyHP,
                    nextPlayerAction
                );

                // 8) 마지막 행동 업데이트
                player.LastAction = nextPlayerAction;
                enemy.LastAction = enemyAction;

                // 9) HP가 0이면 자동 재시작 (전투 끊어짐 방지)
                if (player.HP <= 0 || enemy.HP <= 0)
                {
                    player.HP = 100;
                    enemy.HP = 100;
                    player.LastAction = (EAction)(-1);
                    enemy.LastAction = (EAction)(-1);
                    playerStreak = 0;
                    enemyStreak = 0;
                    turn = 1;
                    continue;
                }

                ++turn;

            }

            Console.WriteLine("10만개 데이터 생성 완료.");
        }

        private int UpdateStreak(EAction lastAction, EAction newAction, int streak)
        {
            if (lastAction == newAction)
                return streak + 1;
            else
                return 1;
        }

        private void UpdateStreak(EAction pa, EAction ea)
        {
            if (pa == player.LastAction)
                playerStreak++;
            else
                playerStreak = 0;

            if (ea == enemy.LastAction)
                enemyStreak++;
            else
                enemyStreak = 0;
        }

        private int CalcDelta(int oldHP, int newHP)
        {
            return newHP - oldHP;
        }



        public async Task StartReal()
        {
            BattleLogger.Initialize(@"D:\AIData\battle_data2.csv");

            turn = 1;
            playerStreak = 0;
            enemyStreak = 0;
            deltaPlayerHP = 0;
            deltaEnemyHP = 0;

            while (player.HP > 0 && enemy.HP > 0)
            {
                ShowStatus();

                // 1) 지난 턴까지의 state 기반으로 Python 호출
                Console.WriteLine("계산중...");
                EAction enemyAction = await GetEnemyActionFromPython();
                enemy.LastAction = enemyAction;

                Console.WriteLine("플레이어 행동 입력:");

                // 2) 플레이어가 이번 턴 행동을 선택
                EAction playerAction = GetPlayerAction();
                player.LastAction = playerAction;

                Console.WriteLine($"pA={pAttack}, pC={pCounter}, pH={pHeal}, result={ActionName(enemyAction)}");
                // 3) 전투 계산
                int oldP = player.HP;
                int oldE = enemy.HP;

                ResolveTurn(playerAction, enemyAction);

                // 4) Delta, Streak, turn 업데이트
                deltaPlayerHP = player.HP - oldP;
                deltaEnemyHP = enemy.HP - oldE;

                UpdateStreak(playerAction, enemyAction);

                turn++;

                Console.WriteLine($"=> 결과: Player {player.HP}, Enemy {enemy.HP}\n");
            }
        }


        private void ShowStatus()
        {
            Console.WriteLine($"\nPlayer HP: {player.HP}, Enemy HP: {enemy.HP}");
            Console.WriteLine("행동 선택: (1) 공격  (2) 반격  (3) 회복");
        }

        private EAction GetPlayerAction()
        {
            while (true)
            {
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value >= 1 && value <= 3)
                {
                    EAction action = (EAction)(value - 1); // 1→0, 2→1, 3→2
                    player.LastAction = action;
                    return action;
                }

                Console.WriteLine("1~3 중 하나만 입력하세요.");
            }
        }

        private async Task<EAction> GetEnemyActionFromPython()
        {
            // 1) Python AI에게 보낼 상태(state) 구성
            var state = new float[]
            {
                player.HP / 100f,             // 1. playerHP
                enemy.HP / 100f,              // 2. enemyHP
                (int)player.LastAction,       // 3. playerLastAction
                (int)enemy.LastAction,        // 4. enemyLastAction
                turn,                   // 5. turnNumber
                playerStreak,                 // 6. playerStreak
                enemyStreak,                  // 7. enemyStreak
                deltaPlayerHP,                // 8. playerDeltaHP
                deltaEnemyHP                  // 9. enemyDeltaHP
            };

            string json = JsonSerializer.Serialize(new { state });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 2) Python 서버 호출
            HttpResponseMessage res = await client.PostAsync("http://localhost:5000/predict", content);
            string body = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);

            // 3) Python이 준 확률 값 받아오기
            pAttack = doc.RootElement.GetProperty("pAttack").GetDouble();
            pCounter = doc.RootElement.GetProperty("pCounter").GetDouble();
            pHeal = doc.RootElement.GetProperty("pHeal").GetDouble();

            // 필요하면 predictedAction도 참고 가능
            // int predicted = doc.RootElement.GetProperty("predictedAction").GetInt32();

            // 4) 머신러닝 예측 → 기대값 기반으로 적 행동 결정
            EAction enemyAction = DecideEnemyActionFromProbabilities();

            enemy.LastAction = enemyAction;

            return enemyAction;
        }


        private EAction DecideEnemyActionFromProbabilities()
        {
            // 기대값 계산 (Enemy 기준)
            double evAttack = -10 * pCounter + 10 * pHeal;
            double evCounter = 10 * pAttack - 20 * pHeal;
            double evHeal = -10 * pAttack + 20 * pCounter;

            double max = evAttack;
            EAction best = EAction.ATTACK;

            if (evCounter > max)
            {
                max = evCounter;
                best = EAction.COUNTER;
            }

            if (evHeal > max)
            {
                max = evHeal;
                best = EAction.HEAL;
            }

            return best;
        }

        void ResolveTurn(EAction _playerAction, EAction _enemyAction)
        {
            const int DAMAGE = 10;
            const int HEAL = 20;

            player.LastAction = _playerAction;
            enemy.LastAction = _enemyAction;

            if (_playerAction == EAction.ATTACK && _enemyAction == EAction.ATTACK) { }
            else if (_playerAction == EAction.ATTACK && _enemyAction == EAction.COUNTER) player.HP -= DAMAGE;
            else if (_playerAction == EAction.ATTACK && _enemyAction == EAction.HEAL) enemy.HP -= DAMAGE;

            else if (_playerAction == EAction.COUNTER && _enemyAction == EAction.ATTACK) enemy.HP -= DAMAGE;
            else if (_playerAction == EAction.COUNTER && _enemyAction == EAction.COUNTER) { }
            else if (_playerAction == EAction.COUNTER && _enemyAction == EAction.HEAL) enemy.HP += HEAL;

            else if (_playerAction == EAction.HEAL && _enemyAction == EAction.ATTACK) player.HP -= DAMAGE;
            else if (_playerAction == EAction.HEAL && _enemyAction == EAction.COUNTER) player.HP += HEAL;
            else if (_playerAction == EAction.HEAL && _enemyAction == EAction.HEAL) { player.HP += HEAL; enemy.HP += HEAL; }

            player.HP = Math.Clamp(player.HP, 0, 100);
            enemy.HP = Math.Clamp(enemy.HP, 0, 100);
        }

        private void ResolveTurn(EAction pa, EAction ea, ref int pHP, ref int eHP)
        {
            const int DMG = 10;
            const int HEAL = 20;

            // 공격 vs 공격 → 아무 일 없음

            if (pa == EAction.ATTACK && ea == EAction.COUNTER)
                pHP -= DMG;
            else if (pa == EAction.ATTACK && ea == EAction.HEAL)
                eHP -= DMG;

            else if (pa == EAction.COUNTER && ea == EAction.ATTACK)
                eHP -= DMG;
            else if (pa == EAction.COUNTER && ea == EAction.HEAL)
                eHP += HEAL;

            else if (pa == EAction.HEAL && ea == EAction.ATTACK)
                pHP -= DMG;
            else if (pa == EAction.HEAL && ea == EAction.COUNTER)
                pHP += HEAL;
            else if (pa == EAction.HEAL && ea == EAction.HEAL)
            {
                pHP += HEAL;
                eHP += HEAL;
            }

            pHP = Math.Clamp(pHP, 0, 100);
            eHP = Math.Clamp(eHP, 0, 100);
        }

        private string ActionName(EAction a)
        {
            return a switch
            {
                EAction.ATTACK => "공격(Attack)",
                EAction.COUNTER => "반격(Counter)",
                EAction.HEAL => "회복(Heal)",
                _ => "???"
            };
        }

        public void RecordState(
            float playerHP,
            float enemyHP,
            EAction playerLastAction,
            EAction enemyLastAction,
            int turnNumber,
            int playerStreak,
            int enemyStreak,
            int playerDeltaHP,
            int enemyDeltaHP,
            EAction playerNextAction
        )
        {
            string line =
                $"{playerHP}," +
                $"{enemyHP}," +
                $"{(int)playerLastAction}," +
                $"{(int)enemyLastAction}," +
                $"{turnNumber}," +
                $"{playerStreak}," +
                $"{enemyStreak}," +
                $"{playerDeltaHP}," +
                $"{enemyDeltaHP}," +
                $"{(int)playerNextAction}";

            BattleLogger.Write(line);
        }
    }
}
