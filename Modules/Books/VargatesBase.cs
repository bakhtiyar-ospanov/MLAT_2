using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace Modules.Books
{
    public class VargatesBase 
    {
        public class Asset
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "inventory")] public string inventory;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        public class MenuItem
        {
            [JsonProperty(PropertyName = "assetId")] public string assetId;
            [JsonProperty(PropertyName = "actions")] public string[] actions;
            [JsonProperty(PropertyName = "name")] public string name;
            public UnityAction call;
        }

        public class Location
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "postfx")] public string postfx;
            [JsonProperty(PropertyName = "camlight")] public string camlight;
        }

        public class Key
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
        }

        [JsonProperty(PropertyName = "Assets")] public List<Asset> assets;
        [JsonProperty(PropertyName = "AssetMenus")] public List<MenuItem> assetMenus;
        [JsonProperty(PropertyName = "Locations")] public List<Location> locations;
        [JsonProperty(PropertyName = "Keys")] public List<Key> keys;

        public Dictionary<string, Asset> assetById;
        public Dictionary<string, Location> locationById;
        public Dictionary<string, List<MenuItem>> assetMenuById;

        public void CreateDictionaries()
        {
            assetById = assets?.Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            locationById = locations?.Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);

            assetMenuById = new Dictionary<string, List<MenuItem>>();

            if (assetMenus != null)
            {
                foreach (var assetMenu in assetMenus)
                {
                    if(!assetMenuById.ContainsKey(assetMenu.assetId))
                        assetMenuById.Add(assetMenu.assetId, new List<MenuItem>());
                
                    assetMenuById[assetMenu.assetId].Add(assetMenu);
                }
            }
        }
    }
}
