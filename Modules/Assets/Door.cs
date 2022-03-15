using System.Collections.Generic;
using Modules.Books;
using Modules.WDCore;

namespace Modules.Assets
{
    public class Door : Asset
    {
        public override void Init()
        {
            base.Init();
            assetMenu ??= new List<MedicalBase.MenuItem>();

            var split = name.Split("_");
            var locationId = split[1];
            var world = split.Length > 2 ? split[2] : null;

            var item = new MedicalBase.MenuItem { 
                name = locationId,
                call = () =>
                {
                    GameManager.Instance.starterController.InitNoWait(locationId, world);
                }};
            
            assetMenu.Add(item);
        }
    }
}
