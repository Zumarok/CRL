using System.Linq;
using Crux.CRL.Utils;
using UnityEditor;
using UnityEngine;

namespace Crux.CRL.Editor
{
    [InitializeOnLoad]
    public class HierarchyColorizer
    {
        private static readonly Vector2 Offset = new Vector2(20, 1);

        static HierarchyColorizer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;
            var component = obj.GetComponent<MonoBehaviour>();

            if (component == null) return;

            var type = component.GetType();
            var attribute = type.GetCustomAttributes(typeof(HierarchyColor), false).FirstOrDefault() as HierarchyColor;

            if (attribute == null) return;


            Rect offsetRect = new Rect(selectionRect.position + Offset, selectionRect.size);
            Rect bgRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width + 50, selectionRect.height);

            EditorGUI.DrawRect(bgRect, attribute.Color);
            EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = new Color(0.1f,0.1f,0.1f)},
                fontStyle = FontStyle.Bold
            });
        }
    }
}