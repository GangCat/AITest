namespace AITest
{
    internal class Program
    {
        static async Task Main(string[] _args)
        {
            BattleMaster battleMaster = new BattleMaster();
            //battleMaster.Start();
            await battleMaster.StartReal();
        }
    }
}