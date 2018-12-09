using System.Collections;
using System.Collections.Generic;

namespace Guanaco
{
    /***************************************************/
    /****      Collection of indexable objects      ****/
    /***************************************************/

    public class IndexedCollection<T> : IEnumerable<T> where T : GuanacoIndexable
    {
        /***************************************************/

        // Fields.
        private Dictionary<int, T> _items = new Dictionary<int, T>() ;
        private int _nextAvailableId = 0;

        /***************************************************/

        // Count the items.
        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        /***************************************************/

        // Add an item to the collection.
        public void Add(T obj, int id = -1)
        {
            if (id == -1)
                id = this._nextAvailableId;

            obj.AssignToCollection(this as IndexedCollection<GuanacoIndexable>, id);
            this._items.Add(id, obj);

            do
            {
                id++;
            }
            while (this._items.ContainsKey(id));

            this._nextAvailableId = id;
        }

        /***************************************************/

        // Remove an item from the collection.
        public void Remove(T obj)
        {
            this.RemoveAt(obj.Id.AsInteger);
        }

        /***************************************************/

        // Remove an item at index.
        public void RemoveAt(int id)
        {
            if (this._items.ContainsKey(id))
            {
                this._items[id].UnassignFromCollection();
                this._items.Remove(id);

                if (id < this._nextAvailableId)
                    this._nextAvailableId = id;
            }
        }

        /***************************************************/

        // Get item at index.
        public T this[int id]
        {
            get
            {
                if (this._items.ContainsKey(id))
                    return this._items[id];
                else
                    return default(T);
            }
        }

        /***************************************************/

        // Enumerators.
        public IEnumerator<T> GetEnumerator()
        {
            foreach(T t in this._items.Values)
            {
                yield return t;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /***************************************************/
    }
}
