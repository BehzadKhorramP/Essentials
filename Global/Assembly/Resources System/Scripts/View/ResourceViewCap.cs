namespace MadApper
{
    public class ResourceViewCap : ResourcesViewTypeBase
    {
        public override string GetText(ResourceData data)
        {
            var amount = data.Amount;
            var cap = data.Cap;

            if (amount < 0)
                amount = 0;
                      
            return amount == cap ? "Full" : $"{amount}/{cap}";
        }
    }
}
