using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AITest
{
    internal class BattleMaster
    {
        Fighter player = new Fighter();
        Fighter enemy = new Fighter();

        const int MAX_TURNS = 10000;

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

                BattleLogger.Write(
                    player.HP / 100f,
                    enemy.HP / 100f,
                    (int)player.LastAction,
                    (int)enemy.LastAction,
                    (int)nextPlayerAction
                );

                if (player.HP <= 0 || enemy.HP <= 0)
                {
                    player.HP = 100;
                    enemy.HP = 100;
                    player.LastAction = EAction.NONE;
                    enemy.LastAction = EAction.NONE;
                }

            }

            Console.WriteLine("1만개 데이터 생성 완료.");
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
    }
}
