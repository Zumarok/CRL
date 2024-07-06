using System;
using UnityEngine;

namespace Crux.CRL.Utils
{
    /// <summary>
    /// Colorize the name in the Unity Hierarchy.
    /// ***Make sure the marked class is the first component on the game object.***
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HierarchyColor : Attribute
    {
        public Color Color { get; set; }

        public HierarchyColor()
        {
            Color = new Color(0.7f, 0.7f, 0.7f);
        }

        public HierarchyColor(float r, float g, float b)
        {
            Color = new Color(r, g, b);
        }
    }
}
