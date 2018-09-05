using System.Collections.Generic;

namespace Guanaco
{
    /***************************************************/
    /****                Library class              ****/
    /***************************************************/

    public static class LibraryUtil
    {
        /***************************************************/

        // Search the library.
        public static GuanacoObject SearchLibrary(string libraryName, string objectName)
        {
            Dictionary<string, GuanacoObject> library = typeof(LibraryUtil).GetField(libraryName).GetValue(null) as Dictionary<string, GuanacoObject>;
            return library == null ? null : library.ContainsKey(objectName) ? library[objectName] : null;
        }

        /***************************************************/

        // Standard material library.
        public static Dictionary<string, GuanacoObject> StandardMaterials = new Dictionary<string, GuanacoObject>
        {
            {"Steel", new MaterialIsotropic("Steel", 210000000000, 0.3, 7860)},
            {"Concrete", new MaterialIsotropic("Concrete", 17000000000, 0.2, 2400)},
            {"Aluminium", new MaterialIsotropic("Aluminium", 69000000000, 0.33, 2700)},
        };
        
        /***************************************************/
    }
}
