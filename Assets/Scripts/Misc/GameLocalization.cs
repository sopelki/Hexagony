namespace Misc
{
    public static class GameLocalization
    {
        public static string GetRussianName(this Logic.Castle.BuildingType type) => type switch
        {
            Logic.Castle.BuildingType.Farm => "Ферма",
            Logic.Castle.BuildingType.Barracks => "Казармы",
            Logic.Castle.BuildingType.Hospital => "Алхимик",
            Logic.Castle.BuildingType.Blacksmith => "Кузница",
            _ => "Здание"
        };

        public static string GetRussianName(this Logic.Tower.TowerType type) => type switch
        {
            Logic.Tower.TowerType.Archer => "Башня лучников",
            Logic.Tower.TowerType.Mage => "Башня магов",
            _ => "Башня"
        };
    }
}