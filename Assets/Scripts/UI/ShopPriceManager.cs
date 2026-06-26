using System.Collections.Generic;
using System.Linq;
using Logic.Castle;

namespace UI
{
    public class ShopPriceManager
    {
        public ShopPriceManager(CastleModel castleModel, List<ShopPriceLabel> labels)
        {
            this.castleModel = castleModel;
            this.labels = labels;
            this.castleModel.OnChanged += UpdateAllLabels;
            UpdateAllLabels();
        }

        private readonly CastleModel castleModel;
        private readonly List<ShopPriceLabel> labels;

        public void Cleanup()
        {
            castleModel.OnChanged -= UpdateAllLabels;
        }

        private void UpdateAllLabels()
        {
            var gold = castleModel.Gold;
            foreach (var label in labels.Where(label => label != null)) label.Refresh(gold);
        }
    }
}