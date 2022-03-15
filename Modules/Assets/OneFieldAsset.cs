using System.Collections.Generic;
using IngameDebugConsole;
using Modules.Books;
using VardixOpenSDK;

namespace Modules.Assets
{
    public class OneFieldAsset : Asset
    {
        private OneField _oneField;
        
        public override void Init()
        {
            base.Init();
            assetMenu ??= new List<MedicalBase.MenuItem>();
            _oneField = GetComponent<OneField>();

            var item = new MedicalBase.MenuItem { 
                name = "",
                call = () =>
                {
                    DebugLogConsole.ExecuteCommand(_oneField.field);
                }};
            
            assetMenu.Add(item);
        }
    }
}
