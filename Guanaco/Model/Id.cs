namespace Guanaco
{
    /***************************************************/
    /****                  Id class                 ****/
    /***************************************************/

    public class GuanacoID
    {
        /***************************************************/

        // Fields & properties.
        private int _asInteger;
        public int AsInteger
        {
            get
            {
                return this._asInteger;
            }
        }

        // Constructor.
        internal GuanacoID(int integerValue)
        {
            this._asInteger = integerValue;
        }

        /***************************************************/
    }
}
