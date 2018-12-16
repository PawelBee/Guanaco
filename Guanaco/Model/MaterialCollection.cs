using System;
using System.Collections.Generic;

namespace Guanaco
{
    /***************************************************/
    /****            Material collection            ****/
    /***************************************************/

    public class MaterialCollection : GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        private List<Material> _items;
        public List<Material> Items
        {
            get
            {
                return this._items;
            }
        }

        /***************************************************/

        // Constructors.
        public MaterialCollection()
        {
            this._items = new List<Material>();
        }

        /***************************************************/

        // Add material to a collection.
        public void Add(Material material)
        {
            foreach (Material m in this._items)
            {
                if (m.Name == material.Name)
                {
                    if (m == material)
                        return;
                    else
                        throw new Exception("Duplicate material names are not allowed. Please change " + material.Name + " to another name.");
                }
            }
            this._items.Add(material);
        }

        /***************************************************/

        // Clone.
        public override GuanacoObject Clone(bool newID = false)
        {
            MaterialCollection mc = this.ShallowClone(newID) as MaterialCollection;
            mc._items = new List<Material>(this._items);
            return mc;
        }

        /***************************************************/
    }
}
