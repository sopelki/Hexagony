namespace Interfaces
{
    public struct TooltipContent
    {
        public string Description;
        public string SpecialInfo;
        public string Title;
    }

    public interface ITooltipProvider
    {
        TooltipContent GetTooltipContent(bool isBought = false);
    }
}