namespace Logic.Castle
{
    public class BuildingModel
    {
        public BuildingModel(BuildingData data)
        {
            Data = data;
            Level = 1;
        }

        public BuildingData Data { get; }
        public int Level { get; private set; }

        // TODO: поменять Production на что-то сложнее
        public int Production => Data.baseProduction * Level;

        public int GetUpgradeCost()
        {
            return Data.baseCost * (Level + 1);
        }

        public void Upgrade()
        {
            Level++;
        }
    }
}