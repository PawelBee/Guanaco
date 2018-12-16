using System;

namespace Guanaco
{
    /***************************************************/
    /****        Generic Guanaco object class       ****/
    /***************************************************/

    public abstract class GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        internal Guid _uniqueId;
        public Guid UniqueId
        {
            get { return this._uniqueId; }
        }

        /***************************************************/

        // Constructors.
        public GuanacoObject()
        {
            this._uniqueId = Guid.NewGuid();
        }

        /***************************************************/

        // Clone.
        public GuanacoObject ShallowClone(bool newID = false)
        {
            GuanacoObject obj = (GuanacoObject)this.MemberwiseClone();

            if (newID)
                obj._uniqueId = Guid.NewGuid();

            return obj;
        }

        public virtual GuanacoObject Clone(bool newID = false)
        {
            return this.ShallowClone(newID);
        }

        /***************************************************/
    }
}
