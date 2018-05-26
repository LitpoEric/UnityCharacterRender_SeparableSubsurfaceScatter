// Amplify Shader Editor - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEditor;

namespace AmplifyShaderEditor
{
	public class EditorVariable<T>
	{
		protected string m_labelName;
		protected string m_name;
		protected T m_value;
		protected T m_defaultValue;

		public EditorVariable( string name, string labelName, T defaultValue ) { m_name = name; m_labelName = labelName; m_defaultValue = defaultValue;m_value = defaultValue; }
		public string Name { get { return m_name; } }

		public virtual T Value
		{
			get { return m_value; }
			set
			{
				m_value = value;
			}
		}
		public string LabelName { get { return m_labelName; } }
	}

	public sealed class EditorVariableFloat : EditorVariable<float>
	{
		public EditorVariableFloat( string name, string labelName, float defaultValue ) : base( name, labelName, defaultValue )
		{
			m_value = EditorPrefs.GetFloat( name, m_defaultValue );
		}

		public override float Value
		{
			get { return m_value; }
			set
			{
				if ( m_value != value )
				{
					m_value = value;
					EditorPrefs.SetFloat( m_name, m_value );
				}
			}
		}
	}

	public sealed class EditorVariableBool : EditorVariable<bool>
	{
		public EditorVariableBool( string name, string labelName, bool defaultValue ) : base( name, labelName, defaultValue )
		{
			m_value = EditorPrefs.GetBool( name, m_defaultValue );
		}

		public override bool Value
		{
			get { return m_value; }
			set
			{
				if ( m_value != value )
				{
					m_value = value;
					EditorPrefs.SetBool( m_name, m_value );
				}
			}
		}
	}

	public sealed class EditorVariableInt : EditorVariable<int>
	{
		public EditorVariableInt( string name, string labelName, int defaultValue ) : base( name, labelName, defaultValue )
		{
			m_value = EditorPrefs.GetInt( name, m_defaultValue );
		}

		public override int Value
		{
			get { return m_value; }
			set
			{
				if ( m_value != value )
				{
					m_value = value;
					EditorPrefs.SetInt( m_name, m_value );
				}
			}
		}
	}

	public sealed class EditorVariableString : EditorVariable<string>
	{
		public EditorVariableString( string name, string labelName, string defaultValue ) : base( name, labelName, defaultValue )
		{
			m_value = EditorPrefs.GetString( name, m_defaultValue );
		}

		public override string Value
		{
			get { return m_value; }
			set
			{
				if ( !m_value.Equals( value ) )
				{
					m_value = value;
					EditorPrefs.SetString( m_name, m_value );
				}
			}
		}
	}

	public class EditorVariablesManager
	{
		public static EditorVariableBool LiveMode = new EditorVariableBool( "ASELiveMode", "LiveMode", false );
		public static EditorVariableBool OutlineActiveMode = new EditorVariableBool( "ASEOutlineActiveMode", " Outline", false );
		public static EditorVariableBool NodeParametersMaximized = new EditorVariableBool( "ASENodeParametersVisible", " NodeParameters", true );
		public static EditorVariableBool NodePaletteMaximized = new EditorVariableBool( "ASENodePaletteVisible", " NodePalette", true );
		public static EditorVariableBool ExpandedRenderingPlatforms = new EditorVariableBool( "ASEExpandedRenderingPlatforms", " ExpandedRenderingPlatforms", false );
		public static EditorVariableBool ExpandedRenderingOptions = new EditorVariableBool( "ASEExpandedRenderingOptions", " ExpandedRenderingPlatforms", false );
		public static EditorVariableBool ExpandedGeneralShaderOptions = new EditorVariableBool( "ASEExpandedGeneralShaderOptions", " ExpandedGeneralShaderOptions", false );
		public static EditorVariableBool ExpandedBlendOptions = new EditorVariableBool( "ASEExpandedBlendOptions", " ExpandedBlendOptions", false );
		public static EditorVariableBool ExpandedStencilOptions = new EditorVariableBool( "ASEExpandedStencilOptions", " ExpandedStencilOptions", false );
		public static EditorVariableBool ExpandedVertexOptions = new EditorVariableBool( "ASEExpandedVertexOptions", " ExpandedVertexOptions", false );
		public static EditorVariableBool ExpandedFunctionInputs = new EditorVariableBool( "ASEExpandedFunctionInputs", " ExpandedFunctionInputs", false );
		public static EditorVariableBool ExpandedFunctionSwitches = new EditorVariableBool( "ASEExpandedFunctionSwitches", " ExpandedFunctionSwitches", false );
		public static EditorVariableBool ExpandedFunctionOutputs = new EditorVariableBool( "ASEExpandedFunctionOutputs", " ExpandedFunctionOutputs", false );
		public static EditorVariableBool ExpandedAdditionalIncludes = new EditorVariableBool( "ASEExpandedAdditionalIncludes", " ExpandedAdditionalIncludes", false );
        public static EditorVariableBool ExpandedAdditionalDefines = new EditorVariableBool( "ASEExpandedAdditionalDefines", " ExpandedAdditionalDefines", false );
        public static EditorVariableBool ExpandedCustomTags = new EditorVariableBool( "ASEExpandedCustomTags", " ExpandedCustomTags", false );
		public static EditorVariableBool ExpandedAdditionalPragmas = new EditorVariableBool( "ASEExpandedAdditionalPragmas", " ExpandedAdditionalPragmas", false );
        public static EditorVariableBool ExpandedDependencies = new EditorVariableBool( "ASEExpandedDependencies", " ExpandedDependencies", false );
        //Templates
        public static EditorVariableBool ExpandedBlendModeModule = new EditorVariableBool( "ASEExpandedBlendModeModule", " ExpandedBlendModeModule", false );
	}
}
