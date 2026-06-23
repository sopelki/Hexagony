namespace Interfaces
{
    public struct TooltipContent
    {
        public string Title;
        public string Description;
        public string SpecialInfo;
    }

    public interface ITooltipProvider
    {
        TooltipContent GetTooltipContent(bool isBought = false);
    }
}