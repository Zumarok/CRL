using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        int buttonsIntValue = 0;
        int enumLength = _property.enumNames.Length;
        bool[] buttonPressed = new bool[enumLength];
        //float buttonWidth = (_position.width - EditorGUIUtility.labelWidth) / enumLength;
        int numOfHorizontalButtons = (int) (EditorGUIUtility.currentViewWidth / (75 + 10));
        EditorGUI.LabelField(new Rect(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height), _label);

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical();
        for (int i = 0; i < enumLength; i++)
        {

            // Check if the button is/was pressed 
            if ((_property.intValue & (1 << i)) == 1 << i)
            {
                buttonPressed[i] = true;
            }
            //Rect buttonPos = new Rect(_position.x + EditorGUIUtility.labelWidth + buttonWidth * i, _position.y, buttonWidth, _position.height);
            //if (i % (int)(EditorGUIUtility.currentViewWidth / (75 + 10)) == 0)
            if (i % numOfHorizontalButtons == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            buttonPressed[i] = GUILayout.Toggle(buttonPressed[i], _property.enumNames[i], "Button", GUILayout.Width(75) );

            if (i == enumLength - 1 || i % numOfHorizontalButtons == numOfHorizontalButtons - 1)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if (buttonPressed[i])
                buttonsIntValue += 1 << i;
        }
        GUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
        {
            _property.intValue = buttonsIntValue;
        }
    }
}