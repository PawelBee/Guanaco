using System.Collections.Generic;

namespace Guanaco
{
    /***************************************************/
    /****             Building component            ****/
    /***************************************************/

    public abstract class BuildingComponent : GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        public abstract int Id { get; }
        public abstract Material Material { get; set; }
        public abstract List<int> Elements { get; }
        public abstract Model ParentModel { get; }

        /***************************************************/

        // Methods to be implemented.
        public abstract List<string> ToCCX();
        public abstract void AssignElements(List<int> elementIds);
        public abstract void AddToModel(Model model);
        public abstract void SetParentModel(Model model);

        /***************************************************/
    }
}
