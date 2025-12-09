using System;
using System.Collections.Generic;
using System.Text;

namespace AITest
{
    internal class BattleLogger
    {
        private static string csvPath = "";

        public static void Initialize(string _fullPath)
        {
            csvPath = _fullPath;

            //if (!File.Exists(csvPath))
            //{
            //    File.WriteAllText(csvPath,
            //        "playerHP,enemyHP,playerLastAction,enemyLastAction,playerNextAction\n");
            //}
            string header = "playerHP,enemyHP,playerLastAction,enemyLastAction,turnNumber,playerStreak,enemyStreak,playerDeltaHP,enemyDeltaHP,playerNextAction";
            File.WriteAllText(csvPath, header + "\n");
        }

        public static void Write(float _pHP, float _eHP, int _pLA, int _eLA, int _nextAction)
        {
            string line = $"{_pHP:F2},{_eHP:F2},{_pLA},{_eLA},{_nextAction}";
            File.AppendAllText(csvPath, line + "\n");
        }

        public static void Write(float _pHP, float _eHP, int _pLA, int _eLA)
        {
            string line = $"{_pHP:F2},{_eHP:F2},{_pLA},{_eLA}";
            File.AppendAllText(csvPath, line + "\n");
        }

        public static void Write(string _line)
        {
            try
            {
                System.IO.File.AppendAllText(csvPath, _line + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CSV ERROR] " + ex.Message);
            }
        }
    }
}
