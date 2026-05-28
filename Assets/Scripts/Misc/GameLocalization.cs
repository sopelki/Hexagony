using Logic.Castle;
using Logic.Tower;
using Logic.Trap;

namespace Misc
{
    public static class GameLocalization
    {
        public static string GetRussianName(this BuildingType type)
        {
            return type switch
            {
                BuildingType.Farm => "Ферма",
                BuildingType.Barracks => "Казарма",
                BuildingType.Hospital => "Алхимик",
                BuildingType.Blacksmith => "Кузница",
                _ => "Здание"
            };
        }

        public static string GetRussianName(this TowerType type)
        {
            return type switch
            {
                TowerType.Archer => "Башня лучников",
                TowerType.Mage => "Башня магов",
                _ => "Башня"
            };
        }

        public static string GetRussianName(this TrapType type)
        {
            return type switch
            {
                TrapType.DamageZone => "Колья",
                TrapType.SlowZone => "Смола",
                TrapType.BearTrap => "Капкан",
                _ => "Ловушка"
            };
        }
    }
}