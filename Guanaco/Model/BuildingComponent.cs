using System.Collections.Generic;

namespace Guanaco
{
    /***************************************************/
    /****             Building component            ****/
    /***************************************************/

    public abstract class BuildingComponent : GuanacoIndexable
    {
        /***************************************************/

        // Fields & properties.
        protected Material _material;
        public virtual Material Material
        {
            get
            {
                return this._material;
            }
            set
            {
                this._material = value;
            }
        }

        protected List<Element> _elements;
        public virtual IEnumerable<Element> Elements
        {
            get
            {
                return this._elements;
            }
        }

        /***************************************************/

        // Methods to be implemented.
        public abstract List<string> ToCCX();
        public abstract void AssignElements(List<Element> elements);

        /***************************************************/
    }
}
