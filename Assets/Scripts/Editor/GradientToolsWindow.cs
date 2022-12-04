using UnityEngine;
using UnityEditor;
using Fab.Common.Editor;

namespace Fab.WorldMod.Gen.Editor
{
	public class GradientToolsWindow : EditorWindow
	{
		private static readonly int MinResolution = 8;
		private static readonly int MaxResolution = 16384;

		public enum OutputFormat
		{
			PNG,
			EXR
		}

		[SerializeField]
		Gradient gradient;

		[SerializeField]
		Texture2D gradientTexture;

		[SerializeField]
		bool customGradientOutputSize;

		[SerializeField]
		int gradientOutputSize = 256;


		[SerializeField]
		Texture2D sourceTexture;

		[SerializeField]
		OutputFormat format;

		[SerializeField]
		bool customOutputResolution;

		[SerializeField]
		Vector2Int resolution = new Vector2Int(512, 512);

		private ReverseGradientGenerator generator;

		[MenuItem("TCD/Gradient Tool")]
		private static void Init()
		{
			GradientToolsWindow window = GetWindow<GradientToolsWindow>();
			window.Show();
		}


		private void OnEnable()
		{
			generator = new ReverseGradientGenerator();
		}

		private void OnDisable()
		{
		}

		private void OnGUI()
		{
			SerializedObject sObj = new SerializedObject(this);
			OnRevereseGradientGUI(sObj);
			GUILayout.Space(32);
			OnCreateGradientGUI(sObj);

		}
		private void OnRevereseGradientGUI(SerializedObject sObj)
        {
			Rect rect = EditorGUILayout.BeginVertical();
			GUI.Box(rect, GUIContent.none);
			GUILayout.Label("Reverse Gradient Tool", EditorStyles.boldLabel);
			EditorGUILayout.ObjectField(sObj.FindProperty(nameof(gradientTexture)));
			EditorGUILayout.ObjectField(sObj.FindProperty(nameof(sourceTexture)));

			EditorGUILayout.PropertyField(sObj.FindProperty(nameof(format)));

			SerializedProperty customResFlagProp = sObj.FindProperty(nameof(customOutputResolution));
			EditorGUILayout.PropertyField(customResFlagProp);

			Vector2Int res;

			if (customResFlagProp.boolValue)
			{
				SerializedProperty outputResProp = sObj.FindProperty(nameof(resolution));
				EditorGUILayout.PropertyField(outputResProp);
				res = outputResProp.vector2IntValue;
				res = new Vector2Int(Mathf.Clamp(res.x, MinResolution, MaxResolution), Mathf.Clamp(res.y, MinResolution, MaxResolution));
				outputResProp.vector2IntValue = res;
			}
			else
			{
				if (sourceTexture)
					res = new Vector2Int(sourceTexture.width, sourceTexture.height);
				else
					res = new Vector2Int(MinResolution, MinResolution);
			}

			sObj.ApplyModifiedProperties();
			GUILayout.Space(13);
			EditorGUI.BeginDisabledGroup(!sourceTexture || !gradientTexture);
			if (GUILayout.Button("Generate"))
			{
				RenderTexture dst = new RenderTexture(
					new RenderTextureDescriptor(
						res.x, res.y,
						format == OutputFormat.EXR ? RenderTextureFormat.RFloat : RenderTextureFormat.R8));

				dst.wrapMode = TextureWrapMode.Repeat;
				dst.enableRandomWrite = true;
				dst.name = sourceTexture.name + "_reverse";

				dst = generator.Generate(gradientTexture, sourceTexture);

				switch (format)
				{
					case OutputFormat.PNG:
						EditorUtils.SaveTexturePNG(dst.ToTexture2D());
						break;
					case OutputFormat.EXR:
						EditorUtils.SaveTextureEXR(dst.ToTexture2D(), Texture2D.EXRFlags.OutputAsFloat);
						break;
					default:
						break;
				}
				dst.Release();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndVertical();
		}

		private static Gradient GetGradient(SerializedProperty gradientProperty)
		{
			System.Reflection.PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			if (propertyInfo == null) { return null; }
			else { return propertyInfo.GetValue(gradientProperty, null) as Gradient; }
		}

		private void OnCreateGradientGUI(SerializedObject sObj)
        {
			Rect rect = EditorGUILayout.BeginVertical();
			GUI.Box(rect, GUIContent.none);
			GUILayout.Label("Create Gradient Tool", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(sObj.FindProperty(nameof(gradient)));

			EditorGUILayout.PropertyField(sObj.FindProperty(nameof(format)));

			SerializedProperty customSizeFlagProp = sObj.FindProperty(nameof(customGradientOutputSize));
			EditorGUILayout.PropertyField(customSizeFlagProp);

			int size = 256;

			if (customSizeFlagProp.boolValue)
			{
				SerializedProperty outputSizeProp = sObj.FindProperty(nameof(gradientOutputSize));
				EditorGUILayout.PropertyField(outputSizeProp);
				size = outputSizeProp.intValue;
				size = Mathf.Clamp(size, MinResolution, MaxResolution);
				outputSizeProp.intValue = size;
			}

			sObj.ApplyModifiedProperties();
			GUILayout.Space(13);
			if (GUILayout.Button("Generate"))
			{
				TextureFormat texFormat = format == OutputFormat.EXR ? TextureFormat.RGBAFloat : TextureFormat.ARGB32;

				Texture2D gradientTex = GenerateGradientTex(gradient, size, texFormat);

				switch (format)
				{
					case OutputFormat.PNG:
						EditorUtils.SaveTexturePNG(gradientTex);
						break;
					case OutputFormat.EXR:
						EditorUtils.SaveTextureEXR(gradientTex, Texture2D.EXRFlags.OutputAsFloat);
						break;
					default:
						break;
				}
			}
			EditorGUILayout.EndVertical();
		}

		private Texture2D GenerateGradientTex(Gradient gradient, int size, TextureFormat format)
        {
			Texture2D gradientTex = new Texture2D(size, 1, format, false);
			Color[] huePickerPixels = new Color[size];
			gradientTex.filterMode = FilterMode.Bilinear;
			gradientTex.wrapMode = TextureWrapMode.Clamp;

			for (int i = 0; i < size; i++)
			{
				huePickerPixels[i] = gradient.Evaluate(i / (size - 1f));
			}

			gradientTex.SetPixels(huePickerPixels);
			gradientTex.Apply();
			return gradientTex;
		}
	}
}