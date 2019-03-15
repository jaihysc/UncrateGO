using System.Collections.Generic;

namespace UncrateGo.Models
{
    public class CsgoContainers
    {
        public List<Container> Containers { get; set; }
    }
    public class Container
    {
        public string Name { get; set; }
        public string CollectionName { get; set; }
        public string IconURL { get; set; }
        public bool IsSticker { get; set; }
        public bool IsSouvenir { get; set; }
        public bool IsTournmentSticker { get; set; }
        public bool SouvenirAvailable { get; set; }
        public List<ContainerEntry> ContainerEntries { get; set; }
    }
    public class ContainerEntry
    {
        //When adding skins
        // 1) Do NOT input the skin wear, wear is automatically looked for
        // 2) Do NOT put a space between the last letter of the skin name and the start of the wear
        // 3) Do NOT input stattrak, Stattrak is automatically accounted for
        public string SkinName { get; set; }
    }

}
