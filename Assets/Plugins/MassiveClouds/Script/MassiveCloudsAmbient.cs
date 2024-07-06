using System;
using UnityEngine;

namespace Mewlist
{
    [Serializable]
    public class MassiveCloudsAmbient
    {
        [ColorUsage(false, true, 0, 1000, 0, 200)]
        [SerializeField] private Color skyColor = Color.blue;
        [ColorUsage(false, true, 0, 1000, 0, 200)]
        [SerializeField] private Color equatorColor = Color.cyan;
        [ColorUsage(false, true, 0, 1000, 0, 200)]
        [SerializeField] private Color groundColor = Color.gray;
        [SerializeField] private float luminanceFix = 0f;

        private readonly Color[] colors = new Color[3] {Color.blue, Color.cyan, Color.gray};

        private Color Fix(Color col)
        {
            var factor = Mathf.Pow(2, -luminanceFix);
            return col / factor;
        }

        public Color SkyColor
        {
            get { return Fix(skyColor); }
            set { skyColor = value; }
        }

        public Color EquatorColor
        {
            get { return Fix(equatorColor); }
            set { equatorColor = value; }
        }

        public Color GroundColor
        {
            get { return Fix(groundColor); }
            set { groundColor = value; }
        }

        public Color[] ToArray()
        {
            colors[0] = skyColor;
            colors[1] = equatorColor;
            colors[2] = groundColor;
            return colors;
        }
    }
}