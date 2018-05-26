using UnityEditor;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	public class OptionsWindow
	{
		private AmplifyShaderEditorWindow m_parentWindow = null;

		private bool m_coloredPorts = true;
		private bool m_multiLinePorts = true;
		private const string MultiLineId = "MultiLinePortsDefault";
		private const string ColorPortId = "ColoredPortsDefault";
		private const string ExpandedStencilId = "ExpandedStencil";
		private const string ExpandedTesselationId = "ExpandedTesselation";
		private const string ExpandedDepthId = "ExpandedDepth";
		private const string ExpandedRenderingOptionsId = "ExpandedRenderingOptions";
		private const string ExpandedRenderingPlatformsId = "ExpandedRenderingPlatforms";
		private const string ExpandedPropertiesId = "ExpandedProperties";
		public OptionsWindow( AmplifyShaderEditorWindow parentWindow )
		{
			m_parentWindow = parentWindow;
			//Load ();
		}

		public void Init()
		{
			Load();
		}

		public void Destroy()
		{
			Save();
		}

		public void Save()
		{
			EditorPrefs.SetBool( ColorPortId, ColoredPorts );
			EditorPrefs.SetBool( MultiLineId, m_multiLinePorts );
			EditorPrefs.SetBool( ExpandedStencilId, ParentWindow.ExpandedStencil );
			EditorPrefs.SetBool( ExpandedTesselationId, ParentWindow.ExpandedTesselation );
			EditorPrefs.SetBool( ExpandedDepthId, ParentWindow.ExpandedDepth );
			EditorPrefs.SetBool( ExpandedRenderingOptionsId, ParentWindow.ExpandedRenderingOptions );
			EditorPrefs.SetBool( ExpandedRenderingPlatformsId, ParentWindow.ExpandedRenderingPlatforms );
			EditorPrefs.SetBool( ExpandedPropertiesId, ParentWindow.ExpandedProperties );
		}

		public void Load()
		{
			ColoredPorts = EditorPrefs.GetBool( ColorPortId, true );
			m_multiLinePorts = EditorPrefs.GetBool( MultiLineId, true );
			ParentWindow.ExpandedStencil = EditorPrefs.GetBool( ExpandedStencilId );
			ParentWindow.ExpandedTesselation = EditorPrefs.GetBool( ExpandedTesselationId );
			ParentWindow.ExpandedDepth = EditorPrefs.GetBool( ExpandedDepthId );
			ParentWindow.ExpandedRenderingOptions = EditorPrefs.GetBool( ExpandedRenderingOptionsId );
			ParentWindow.ExpandedRenderingPlatforms = EditorPrefs.GetBool( ExpandedRenderingPlatformsId );
			ParentWindow.ExpandedProperties = EditorPrefs.GetBool( ExpandedPropertiesId );
		}

		public bool ColoredPorts
		{
			get { return m_coloredPorts; }
			set
			{
				if ( m_coloredPorts != value )
					EditorPrefs.SetBool( ColorPortId, value );

				m_coloredPorts = value;
			}
		}

		public bool MultiLinePorts
		{
			get { return m_multiLinePorts; }
			set
			{
				if ( m_multiLinePorts != value )
					EditorPrefs.SetBool( MultiLineId, value );

				m_multiLinePorts = value;
			}
		}

		public AmplifyShaderEditorWindow ParentWindow { get { return m_parentWindow; } set { m_parentWindow = value; } }
	}
}
