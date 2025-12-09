namespace AITest
{
    internal class Program
    {
        static void Main(string[] _args)
        {
            BattleMaster battleMaster = new BattleMaster();
            var enumerator = battleMaster.Start();
            while (enumerator.MoveNext()) { }
        }
    }
}