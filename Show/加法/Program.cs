class Program
{
    static void Main()
    {
        Console.WriteLine("请输入企业的资产总额（元）：");
        string assetInput = Console.ReadLine();
        if (!decimal.TryParse(assetInput, out decimal totalAssets) || totalAssets < 0)
        {
            Console.WriteLine("资产输入无效，请输入非负数字！");
            return;
        }

        Console.WriteLine("请输入企业的负债总额（元）：");
        string liabilityInput = Console.ReadLine();
        if (!decimal.TryParse(liabilityInput, out decimal totalLiabilities) || totalLiabilities < 0)
        {
            Console.WriteLine("负债输入无效，请输入非负数字！");
            return;
        }

        if (totalLiabilities > totalAssets)
        {
            Console.WriteLine("警告：负债总额大于资产总额，企业可能负债超标！");
        }

        decimal netAssets = totalAssets - totalLiabilities;

        Console.WriteLine($"企业净资产（股东权益）为：{netAssets:C2}");
    }
}
