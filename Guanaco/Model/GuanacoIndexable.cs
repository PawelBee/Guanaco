using System;

namespace Guanaco
{
    /***************************************************/
    /****             Indexable object              ****/
    /***************************************************/

    public abstract class GuanacoIndexable: GuanacoObject
    {
        /***************************************************/

        // Fields & properties.
        protected GuanacoID _id;
        public GuanacoID Id
        {
            get
            {
                return this._id;
            }
        }

        internal IndexedCollection<GuanacoIndexable> _parentCollection;

        /***************************************************/

        // Assign the object to a collection.
        internal void AssignToCollection(IndexedCollection<GuanacoIndexable> collection, int id)
        {
            if (this._parentCollection != null || this._id != null)
                throw new Exception("This object already belongs to a collection.");

            this._parentCollection = collection;
            this._id = new GuanacoID(id);
        }

        /***************************************************/

        // Unassign the object from a collection.
        internal void UnassignFromCollection()
        {
            this._parentCollection = null;
            this._id = null;
        }

        /***************************************************/

        // Convert Id to CCX Id (zero is not allowed).
        public virtual string CCXId()
        {
            return (this.Id.AsInteger + 1).ToString();
        }

        /***************************************************/
    }
}
