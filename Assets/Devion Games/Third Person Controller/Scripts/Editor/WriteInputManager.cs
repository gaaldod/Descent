using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DevionGames
{
	[InitializeOnLoad]
	public class WriteInputManager
	{
		static WriteInputManager ()
		{
			if (!AxisDefined ("Change Speed")) {
				AddAxis (new InputAxis () {
					name = "Change Speed",
					positiveButton = "left shift",
					gravity = 1000,
					dead = 0.1f,
					sensitivity = 1000f,
					type = AxisType.KeyOrMouseButton,
					axis = 1
				});
			}
			if (!AxisDefined ("Crouch")) {
				AddAxis (new InputAxis () {
					name = "Crouch",
					positiveButton = "c",
					gravity = 1000,
					dead = 0.1f,
					sensitivity = 1000f,
					type = AxisType.KeyOrMouseButton,
					axis = 1
				});
			}
			if (!AxisDefined("No Control"))
			{
				AddAxis(new InputAxis()
				{
					name = "No Control",
					positiveButton = "left ctrl",
					gravity = 1000,
					dead = 0.1f,
					sensitivity = 1000f,
					type = AxisType.KeyOrMouseButton,
					axis = 1
				});
			}
			if (!AxisDefined("Evade"))
			{
				AddAxis(new InputAxis()
				{
					name = "Evade",
					positiveButton = "left alt",
					gravity = 1000,
					dead = 0.1f,
					sensitivity = 1000f,
					type = AxisType.KeyOrMouseButton,
					axis = 1
				});
			}
		}

		private static SerializedProperty GetChildProperty (SerializedProperty parent, string name)
		{
			if (parent == null)
				return null;
				
			SerializedProperty child = parent.Copy ();
			if (!child.Next (true))
				return null;
				
			do {
				if (child.name == name)
					return child;
			} while (child.Next (false));
			return null;
		}

		private static bool AxisDefined (string axisName)
		{
			try
			{
				SerializedObject serializedObject = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/InputManager.asset") [0]);
				SerializedProperty axesProperty = serializedObject.FindProperty ("m_Axes");

				if (axesProperty == null || !axesProperty.isArray)
					return false;

				axesProperty.Next (true);
				axesProperty.Next (true);
				while (axesProperty.Next (false)) {
					SerializedProperty axis = axesProperty.Copy ();
					if (axis.Next (true)) {
						if (axis.stringValue == axisName)
							return true;
					}
				}
			}
			catch (System.Exception)
			{
				// Silently fail - Input Manager might not be accessible
				return false;
			}
			return false;
		}

		public enum AxisType
		{
			KeyOrMouseButton = 0,
			MouseMovement = 1,
			JoystickAxis = 2
		};

		public class InputAxis
		{
			public string name;
			public string descriptiveName;
			public string descriptiveNegativeName;
			public string negativeButton;
			public string positiveButton;
			public string altNegativeButton;
			public string altPositiveButton;

			public float gravity;
			public float dead;
			public float sensitivity;

			public bool snap = false;
			public bool invert = false;

			public AxisType type;

			public int axis;
			public int joyNum;
		}

		private static void AddAxis (InputAxis axis)
		{
			if (AxisDefined (axis.name))
				return;

			try
			{
				SerializedObject serializedObject = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/InputManager.asset") [0]);
				SerializedProperty axesProperty = serializedObject.FindProperty ("m_Axes");

				if (axesProperty == null || !axesProperty.isArray)
					return;

				axesProperty.arraySize++;
				serializedObject.ApplyModifiedProperties ();

				SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex (axesProperty.arraySize - 1);

				SerializedProperty nameProp = GetChildProperty (axisProperty, "m_Name");
				if (nameProp != null) nameProp.stringValue = axis.name;
				
				SerializedProperty descProp = GetChildProperty (axisProperty, "descriptiveName");
				if (descProp != null) descProp.stringValue = axis.descriptiveName;
				
				SerializedProperty descNegProp = GetChildProperty (axisProperty, "descriptiveNegativeName");
				if (descNegProp != null) descNegProp.stringValue = axis.descriptiveNegativeName;
				
				SerializedProperty negButtonProp = GetChildProperty (axisProperty, "negativeButton");
				if (negButtonProp != null) negButtonProp.stringValue = axis.negativeButton;
				
				SerializedProperty posButtonProp = GetChildProperty (axisProperty, "positiveButton");
				if (posButtonProp != null) posButtonProp.stringValue = axis.positiveButton;
				
				SerializedProperty altNegButtonProp = GetChildProperty (axisProperty, "altNegativeButton");
				if (altNegButtonProp != null) altNegButtonProp.stringValue = axis.altNegativeButton;
				
				SerializedProperty altPosButtonProp = GetChildProperty (axisProperty, "altPositiveButton");
				if (altPosButtonProp != null) altPosButtonProp.stringValue = axis.altPositiveButton;
				
				SerializedProperty gravityProp = GetChildProperty (axisProperty, "gravity");
				if (gravityProp != null) gravityProp.floatValue = axis.gravity;
				
				SerializedProperty deadProp = GetChildProperty (axisProperty, "dead");
				if (deadProp != null) deadProp.floatValue = axis.dead;
				
				SerializedProperty sensitivityProp = GetChildProperty (axisProperty, "sensitivity");
				if (sensitivityProp != null) sensitivityProp.floatValue = axis.sensitivity;
				
				SerializedProperty snapProp = GetChildProperty (axisProperty, "snap");
				if (snapProp != null) snapProp.boolValue = axis.snap;
				
				SerializedProperty invertProp = GetChildProperty (axisProperty, "invert");
				if (invertProp != null) invertProp.boolValue = axis.invert;
				
				SerializedProperty typeProp = GetChildProperty (axisProperty, "type");
				if (typeProp != null) typeProp.intValue = (int)axis.type;
				
				SerializedProperty axisProp = GetChildProperty (axisProperty, "axis");
				if (axisProp != null) axisProp.intValue = axis.axis - 1;
				
				SerializedProperty joyNumProp = GetChildProperty (axisProperty, "joyNum");
				if (joyNumProp != null) joyNumProp.intValue = axis.joyNum;

				serializedObject.ApplyModifiedProperties ();
			}
			catch (System.Exception)
			{
				// Silently fail - Input Manager might not be accessible
			}
		}
	}
}