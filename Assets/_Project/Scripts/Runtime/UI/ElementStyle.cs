using UnityEngine.UIElements;
using Spaa.Elements;

namespace Spaa.UI
{
    public static class ElementStyle
    {
        private static readonly string[] Classes = { "el--rest", "el--selfcare", "el--food", "el--rebuild", "el--move" };

        public static string ClassFor(Element element)
        {
            return Classes[(int)element];
        }

        public static int ClassCount => Classes.Length;

        public static void Apply(VisualElement target, Element element)
        {
            string wanted = ClassFor(element);
            for (int i = 0; i < Classes.Length; i++)
            {
                target.EnableInClassList(Classes[i], Classes[i] == wanted);
            }
        }

        public static void Clear(VisualElement target)
        {
            for (int i = 0; i < Classes.Length; i++)
            {
                target.RemoveFromClassList(Classes[i]);
            }
        }
    }
}
