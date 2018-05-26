// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Parallax Occlusion Mapping", "UV Coordinates", "Calculates offseted UVs for parallax occlusion mapping" )]
	public sealed class ParallaxOcclusionMappingNode : ParentNode
	{
		[SerializeField]
		private int m_selectedChannelInt = 0;

		[SerializeField]
		private int m_minSamples = 8;

		[SerializeField]
		private int m_maxSamples = 16;

		[SerializeField]
		private int m_sidewallSteps = 2;

		[SerializeField]
		private float m_defaultScale = 0.02f;

		[SerializeField]
		private float m_defaultRefPlane = 0f;

		[SerializeField]
		private bool m_clipEnds = false;

		[SerializeField]
		private Vector2 m_tilling = new Vector2( 1, 1 );

		[SerializeField]
		private bool m_useCurvature = false;

		[SerializeField]
		private bool m_useTextureArray = false;

		//[SerializeField]
		//private bool m_useCurvature = false;

		[SerializeField]
		private Vector2 m_CurvatureVector = new Vector2( 0, 0 );

		private readonly string[] m_channelTypeStr = { "Red Channel", "Green Channel", "Blue Channel", "Alpha Channel" };
		private readonly string[] m_channelTypeVal = { "r", "g", "b", "a" };

		private string m_functionHeader = "POM( {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13} )";
		private string m_functionBody = string.Empty;

		private const string WorldDirVarStr = "worldViewDir";
		//private readonly string WorldDirVarDecStr = "{0} {1};";
		//private readonly string WorldDirVarDefStr = string.Format( "{0}.{1} = normalize( _WorldSpaceCameraPos - {2}.vertex )", Constants.VertexShaderOutputStr, WorldDirVarStr, Constants.VertexShaderInputStr );
		//private readonly string WorldDirVarOnFrag = Constants.InputVarStr + "." + WorldDirVarStr;

		private InputPort m_uvPort;
		private InputPort m_texPort;
		private InputPort m_scalePort;
		private InputPort m_viewdirTanPort;
		private InputPort m_refPlanePort;
		private InputPort m_curvaturePort;
		private InputPort m_arrayIndexPort;

		private OutputPort m_pomUVPort;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT2, false, "UV" );
			AddInputPort( WirePortDataType.SAMPLER2D, false, "Tex" );
			AddInputPort( WirePortDataType.FLOAT, false, "Scale" );
			AddInputPort( WirePortDataType.FLOAT3, false, "ViewDir (tan)" );
			AddInputPort( WirePortDataType.FLOAT, false, "Ref Plane" );
			AddInputPort( WirePortDataType.FLOAT2, false, "Curvature" );
			AddInputPort( WirePortDataType.FLOAT, false, "Array Index" );
			AddOutputPort( WirePortDataType.FLOAT2, "Out" );

			m_uvPort = m_inputPorts[ 0 ];
			m_texPort = m_inputPorts[ 1 ];
			m_scalePort = m_inputPorts[ 2 ];
			m_viewdirTanPort = m_inputPorts[ 3 ];
			m_refPlanePort = m_inputPorts[ 4 ];
			m_pomUVPort = m_outputPorts[ 0 ];
			m_curvaturePort = m_inputPorts[ 5 ];
			m_arrayIndexPort = m_inputPorts[ 6 ];
			m_scalePort.FloatInternalData = 0.02f;
			m_useInternalPortData = false;
			m_textLabelWidth = 130;
			m_autoWrapProperties = true;
			m_curvaturePort.Visible = false;
			m_arrayIndexPort.Visible = false;
			UpdateSampler();
		}

		public override void DrawProperties()
		{
			base.DrawProperties();

			EditorGUI.BeginChangeCheck();
			m_selectedChannelInt = EditorGUILayoutPopup( "Channel", m_selectedChannelInt, m_channelTypeStr );
			if ( EditorGUI.EndChangeCheck() )
			{
				UpdateSampler();
				GeneratePOMfunction();
			}
			EditorGUIUtility.labelWidth = 105;

			m_minSamples = EditorGUILayoutIntSlider( "Min Samples", m_minSamples, 1, 128 );
			m_maxSamples = EditorGUILayoutIntSlider( "Max Samples", m_maxSamples, 1, 128 );
			
			EditorGUI.BeginChangeCheck();
			m_sidewallSteps = EditorGUILayoutIntSlider( "Sidewall Steps", m_sidewallSteps, 0, 10 );
			if ( EditorGUI.EndChangeCheck() )
			{
				GeneratePOMfunction();
			}


			EditorGUI.BeginDisabledGroup(m_scalePort.IsConnected );
			m_defaultScale = EditorGUILayoutSlider( "Default Scale", m_defaultScale, 0, 1 );
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup( m_refPlanePort.IsConnected );
			m_defaultRefPlane = EditorGUILayoutSlider( "Default Ref Plane", m_defaultRefPlane, 0, 1 );
			EditorGUI.EndDisabledGroup();
			EditorGUIUtility.labelWidth = m_textLabelWidth;
			EditorGUI.BeginChangeCheck();
			m_useTextureArray = EditorGUILayoutToggle( "Use Texture Array", m_useTextureArray );
			if( EditorGUI.EndChangeCheck() )
			{
				m_arrayIndexPort.Visible = m_useTextureArray;
				m_sizeIsDirty = true;
				GeneratePOMfunction();
				//UpdateCurvaturePort();
			}

			if( m_useTextureArray && !m_arrayIndexPort.IsConnected )
			{
				m_arrayIndexPort.FloatInternalData = EditorGUILayoutFloatField( "Array Index", m_arrayIndexPort.FloatInternalData );
			}

			//float cached = EditorGUIUtility.labelWidth;
			//EditorGUIUtility.labelWidth = 70;
			m_clipEnds = EditorGUILayoutToggle( "Clip Edges", m_clipEnds );
			//EditorGUIUtility.labelWidth = -1;
			//EditorGUIUtility.labelWidth = 100;
			//EditorGUILayout.BeginHorizontal();
			//EditorGUI.BeginDisabledGroup( !m_clipEnds );
			//m_tilling = EditorGUILayout.Vector2Field( string.Empty, m_tilling );
			//EditorGUI.EndDisabledGroup();
			//EditorGUILayout.EndHorizontal();
			//EditorGUIUtility.labelWidth = cached;

			EditorGUI.BeginChangeCheck();
			m_useCurvature = EditorGUILayoutToggle( "Clip Silhouette", m_useCurvature );
			if ( EditorGUI.EndChangeCheck() )
			{
				GeneratePOMfunction();
				UpdateCurvaturePort();
			}

			EditorGUI.BeginDisabledGroup( !(m_useCurvature && !m_curvaturePort.IsConnected) );
			m_CurvatureVector = EditorGUILayoutVector2Field( string.Empty, m_CurvatureVector );
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.HelpBox( "Min and Max samples:\nControl the minimum and maximum number of layers extruded\n\nSidewall Steps:\nThe number of interpolations done to smooth the extrusion result on the side of the layer extrusions, min is used at steep angles while max is used at orthogonal angles\n\n"+
				"Ref Plane:\nReference plane lets you adjust the starting reference height, 0 = deepen ground, 1 = raise ground, any value above 0 might cause distortions at higher angles\n\n"+
				"Clip Edges:\nThis will clip the ends of your uvs to give a more 3D look at the edges. It'll use the tilling given by your Heightmap input.\n\n"+
				"Clip Silhouette:\nTurning this on allows you to use the UV coordinates to clip the effect curvature in U or V axis, useful for cylinders, works best with 'Clip Edges' turned OFF", MessageType.None );
		}

		private void UpdateSampler()
		{
			m_texPort.Name = "Tex (" + m_channelTypeVal[ m_selectedChannelInt ].ToUpper() + ")";
		}

		private void UpdateCurvaturePort()
		{
			if ( m_useCurvature )
				m_curvaturePort.Visible = true;
			else
				m_curvaturePort.Visible = false;

			m_sizeIsDirty = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );

			GeneratePOMfunction();

			string textcoords = m_uvPort.GeneratePortInstructions( ref dataCollector );
			string texture = m_texPort.GeneratePortInstructions( ref dataCollector );
			string scale = m_defaultScale.ToString();
			if( m_scalePort.IsConnected )
				scale = m_scalePort.GeneratePortInstructions( ref dataCollector );

			string viewDirTan = "";
			if ( !m_viewdirTanPort.IsConnected )
			{
				if ( !dataCollector.DirtyNormal )
					dataCollector.ForceNormal = true;

				
				if ( dataCollector.IsTemplate )
				{
					viewDirTan = dataCollector.TemplateDataCollectorInstance.GetTangentViewDir( m_currentPrecisionType );
				}
				else
				{
					dataCollector.AddToInput( UniqueId, SurfaceInputs.VIEW_DIR, m_currentPrecisionType );
					viewDirTan = Constants.InputVarStr + "." + UIUtils.GetInputValueFromType( SurfaceInputs.VIEW_DIR );
				}
			}
			else
			{
				viewDirTan = m_viewdirTanPort.GeneratePortInstructions( ref dataCollector );
			}

			//generate world normal
			string normalWorld = string.Empty;
			if ( dataCollector.IsTemplate )
			{
				normalWorld = dataCollector.TemplateDataCollectorInstance.GetWorldNormal( m_currentPrecisionType );
			}
			else
			{
				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, m_currentPrecisionType );
				dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );
				normalWorld = GeneratorUtils.GenerateWorldNormal( ref dataCollector, UniqueId );
			}

			//string normalWorld = "WorldNormalVector( " + Constants.InputVarStr + ", float3( 0, 0, 1 ) )";

			//generate viewDir in world space

			string worldPos = string.Empty;
			if ( dataCollector.IsTemplate )
			{
				worldPos = dataCollector.TemplateDataCollectorInstance.GetWorldPos();
			}
			else
			{
				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_POS );
				worldPos = Constants.InputVarStr + ".worldPos";
			}
			dataCollector.AddToLocalVariables( UniqueId, m_currentPrecisionType, WirePortDataType.FLOAT3, WorldDirVarStr, string.Format( "normalize( UnityWorldSpaceViewDir( {0} ) )", worldPos ) );
			
			//dataCollector.AddToInput( m_uniqueId, string.Format( WorldDirVarDecStr, UIUtils.FinalPrecisionWirePortToCgType( m_currentPrecisionType, WirePortDataType.FLOAT3 ), WorldDirVarStr ), false );
			//dataCollector.AddVertexInstruction( WorldDirVarDefStr, m_uniqueId );

			string dx = "ddx("+ textcoords + ")";
			string dy = "ddx(" + textcoords + ")";

			string refPlane = m_defaultRefPlane.ToString();
			if ( m_refPlanePort.IsConnected )
				refPlane = m_refPlanePort.GeneratePortInstructions( ref dataCollector );


			string curvature = "float2("+ m_CurvatureVector.x + "," + m_CurvatureVector.y + ")";
			if ( m_useCurvature )
			{
				dataCollector.AddToProperties( UniqueId, "[Header(Parallax Occlusion Mapping)]", 300 );
				dataCollector.AddToProperties( UniqueId, "_CurvFix(\"Curvature Bias\", Range( 0 , 1)) = 1", 301 );
				dataCollector.AddToUniforms( UniqueId, "uniform float _CurvFix;" );

				if ( m_curvaturePort.IsConnected )
					curvature = m_curvaturePort.GeneratePortInstructions( ref dataCollector );
			}


			string localVarName = "OffsetPOM" + UniqueId;
			dataCollector.AddToUniforms(UniqueId, "uniform float4 "+ texture +"_ST;");

			string arrayIndex = m_arrayIndexPort.GeneratePortInstructions( ref dataCollector );

			if( m_useTextureArray )
				dataCollector.UsingArrayDerivatives = true;

			string functionResult = dataCollector.AddFunctions( m_functionHeader, m_functionBody, (m_useTextureArray ? "UNITY_PASS_TEX2DARRAY(" + texture + ")": texture), textcoords, dx, dy, normalWorld, WorldDirVarStr, viewDirTan, m_minSamples, m_maxSamples, scale, refPlane, texture+"_ST.xy", curvature, arrayIndex );

			dataCollector.AddToLocalVariables( UniqueId, m_currentPrecisionType, m_pomUVPort.DataType, localVarName, functionResult );

			return GetOutputVectorItem( 0, outputId, localVarName );
		}

		private void GeneratePOMfunction()
		{
			m_functionBody = string.Empty;
			if(	m_useTextureArray )
				IOUtils.AddFunctionHeader( ref m_functionBody, "inline float2 POM( UNITY_ARGS_TEX2DARRAY(heightMap), float2 uvs, float2 dx, float2 dy, float3 normalWorld, float3 viewWorld, float3 viewDirTan, int minSamples, int maxSamples, float parallax, float refPlane, float2 tilling, float2 curv, int index )" );
			else
				IOUtils.AddFunctionHeader( ref m_functionBody, "inline float2 POM( sampler2D heightMap, float2 uvs, float2 dx, float2 dy, float3 normalWorld, float3 viewWorld, float3 viewDirTan, int minSamples, int maxSamples, float parallax, float refPlane, float2 tilling, float2 curv, int index )" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float3 result = 0;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "int stepIndex = 0;" );
			//IOUtils.AddFunctionLine( ref m_functionBody, "int numSteps = ( int )( minSamples + dot( viewWorld, normalWorld ) * ( maxSamples - minSamples ) );" );
			//IOUtils.AddFunctionLine( ref m_functionBody, "int numSteps = ( int )lerp( maxSamples, minSamples, length( fwidth( uvs ) ) * 10 );" );
			IOUtils.AddFunctionLine( ref m_functionBody, "int numSteps = ( int )lerp( (float)maxSamples, (float)minSamples, (float)dot( normalWorld, viewWorld ) );" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float layerHeight = 1.0 / numSteps;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float2 plane = parallax * ( viewDirTan.xy / viewDirTan.z );" );
			IOUtils.AddFunctionLine( ref m_functionBody, "uvs += refPlane * plane;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float2 deltaTex = -plane * layerHeight;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float2 prevTexOffset = 0;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float prevRayZ = 1.0f;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float prevHeight = 0.0f;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float2 currTexOffset = deltaTex;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float currRayZ = 1.0f - layerHeight;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float currHeight = 0.0f;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float intersection = 0;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "float2 finalTexOffset = 0;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "while ( stepIndex < numSteps + 1 )" );
			IOUtils.AddFunctionLine( ref m_functionBody, "{" );
			if( m_useCurvature )
			{
				IOUtils.AddFunctionLine( ref m_functionBody, "	result.z = dot( curv, currTexOffset * currTexOffset );" );
				if( m_useTextureArray )
					IOUtils.AddFunctionLine( ref m_functionBody, "	currHeight = ASE_SAMPLE_TEX2DARRAY_GRAD( heightMap, float3(uvs + currTexOffset,index), dx, dy )." + m_channelTypeVal[ m_selectedChannelInt ] + " * ( 1 - result.z );" );
				else
					IOUtils.AddFunctionLine( ref m_functionBody, "	currHeight = tex2Dgrad( heightMap, uvs + currTexOffset, dx, dy )." + m_channelTypeVal[ m_selectedChannelInt ] + " * ( 1 - result.z );" );
			}
			else
			{
				if( m_useTextureArray )
					IOUtils.AddFunctionLine( ref m_functionBody, "	currHeight = ASE_SAMPLE_TEX2DARRAY_GRAD( heightMap,  float3(uvs + currTexOffset,index), dx, dy )." + m_channelTypeVal[ m_selectedChannelInt ] + ";" );
				else
					IOUtils.AddFunctionLine( ref m_functionBody, "	currHeight = tex2Dgrad( heightMap, uvs + currTexOffset, dx, dy )." + m_channelTypeVal[ m_selectedChannelInt ] + ";" );
			}
			IOUtils.AddFunctionLine( ref m_functionBody, "	if ( currHeight > currRayZ )" );
			IOUtils.AddFunctionLine( ref m_functionBody, "	{" );
			IOUtils.AddFunctionLine( ref m_functionBody, "		stepIndex = numSteps + 1;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "	}" );
			IOUtils.AddFunctionLine( ref m_functionBody, "	else" );
			IOUtils.AddFunctionLine( ref m_functionBody, "	{" );
			IOUtils.AddFunctionLine( ref m_functionBody, "		stepIndex++;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "		prevTexOffset = currTexOffset;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "		prevRayZ = currRayZ;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "		prevHeight = currHeight;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "		currTexOffset += deltaTex;" );
			if ( m_useCurvature )
				IOUtils.AddFunctionLine( ref m_functionBody, "		currRayZ -= layerHeight * ( 1 - result.z ) * (1+_CurvFix);" );
			else
				IOUtils.AddFunctionLine( ref m_functionBody, "		currRayZ -= layerHeight;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "	}" );
			IOUtils.AddFunctionLine( ref m_functionBody, "}" );

			if ( m_sidewallSteps > 0 )
			{
				IOUtils.AddFunctionLine( ref m_functionBody, "int sectionSteps = " + m_sidewallSteps + ";" );
				IOUtils.AddFunctionLine( ref m_functionBody, "int sectionIndex = 0;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "float newZ = 0;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "float newHeight = 0;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "while ( sectionIndex < sectionSteps )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "{" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	intersection = ( prevHeight - prevRayZ ) / ( prevHeight - currHeight + currRayZ - prevRayZ );" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	finalTexOffset = prevTexOffset + intersection * deltaTex;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	newZ = prevRayZ - intersection * layerHeight;" );
				if( m_useTextureArray )
					IOUtils.AddFunctionLine( ref m_functionBody, "	newHeight = ASE_SAMPLE_TEX2DARRAY_GRAD( heightMap, float3(uvs + finalTexOffset,index), dx, dy )." + m_channelTypeVal[ m_selectedChannelInt ] + ";" );
				else
					IOUtils.AddFunctionLine( ref m_functionBody, "	newHeight = tex2Dgrad( heightMap, uvs + finalTexOffset, dx, dy )." + m_channelTypeVal[ m_selectedChannelInt ] + ";" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	if ( newHeight > newZ )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	{" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		currTexOffset = finalTexOffset;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		currHeight = newHeight;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		currRayZ = newZ;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		deltaTex = intersection * deltaTex;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		layerHeight = intersection * layerHeight;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	}" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	else" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	{" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		prevTexOffset = finalTexOffset;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		prevHeight = newHeight;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		prevRayZ = newZ;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		deltaTex = ( 1 - intersection ) * deltaTex;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		layerHeight = ( 1 - intersection ) * layerHeight;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	}" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	sectionIndex++;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "}" );
			}
			else
			{
				IOUtils.AddFunctionLine( ref m_functionBody, "finalTexOffset = currTexOffset;" );
			}

			if ( m_useCurvature )
			{
				IOUtils.AddFunctionLine( ref m_functionBody, "#ifdef UNITY_PASS_SHADOWCASTER" );
				IOUtils.AddFunctionLine( ref m_functionBody, "if ( unity_LightShadowBias.z == 0.0 )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "{" );
				IOUtils.AddFunctionLine( ref m_functionBody, "#endif" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	if ( result.z > 1 )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		clip( -1 );" );
				IOUtils.AddFunctionLine( ref m_functionBody, "#ifdef UNITY_PASS_SHADOWCASTER" );
				IOUtils.AddFunctionLine( ref m_functionBody, "}" );
				IOUtils.AddFunctionLine( ref m_functionBody, "#endif" );
			}

			if ( m_clipEnds )
			{
				IOUtils.AddFunctionLine( ref m_functionBody, "result.xy = uvs + finalTexOffset;" );
				IOUtils.AddFunctionLine( ref m_functionBody, "#ifdef UNITY_PASS_SHADOWCASTER" );
				IOUtils.AddFunctionLine( ref m_functionBody, "if ( unity_LightShadowBias.z == 0.0 )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "{" );
				IOUtils.AddFunctionLine( ref m_functionBody, "#endif" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	if ( result.x < 0 )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		clip( -1 );" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	if ( result.x > tilling.x )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		clip( -1 );" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	if ( result.y < 0 )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		clip( -1 );" );
				IOUtils.AddFunctionLine( ref m_functionBody, "	if ( result.y > tilling.y )" );
				IOUtils.AddFunctionLine( ref m_functionBody, "		clip( -1 );" );
				IOUtils.AddFunctionLine( ref m_functionBody, "#ifdef UNITY_PASS_SHADOWCASTER" );
				IOUtils.AddFunctionLine( ref m_functionBody, "}" );
				IOUtils.AddFunctionLine( ref m_functionBody, "#endif" );
				IOUtils.AddFunctionLine( ref m_functionBody, "return result.xy;" );
			}
			else
			{
				IOUtils.AddFunctionLine( ref m_functionBody, "return uvs + finalTexOffset;" );
			}
			IOUtils.CloseFunctionBody( ref m_functionBody );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_selectedChannelInt = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_minSamples = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_maxSamples = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_sidewallSteps = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_defaultScale = Convert.ToSingle( GetCurrentParam( ref nodeParams ) );
			m_defaultRefPlane = Convert.ToSingle( GetCurrentParam( ref nodeParams ) );
			if ( UIUtils.CurrentShaderVersion() > 3001 )
			{
				m_clipEnds = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
				string[] vector2Component = GetCurrentParam( ref nodeParams ).Split( IOUtils.VECTOR_SEPARATOR );
				if ( vector2Component.Length == 2 )
				{
					m_tilling.x = Convert.ToSingle( vector2Component[ 0 ] );
					m_tilling.y = Convert.ToSingle( vector2Component[ 1 ] );
				}
			}

			if ( UIUtils.CurrentShaderVersion() > 5005 )
			{
				m_useCurvature = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
				m_CurvatureVector = IOUtils.StringToVector2( GetCurrentParam( ref nodeParams ) );
			}

			if( UIUtils.CurrentShaderVersion() > 13103 )
			{
				m_useTextureArray = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
				m_arrayIndexPort.Visible = m_useTextureArray;
			}

			UpdateSampler();
			GeneratePOMfunction();
			UpdateCurvaturePort();
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_selectedChannelInt );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_minSamples );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_maxSamples );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_sidewallSteps );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_defaultScale );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_defaultRefPlane );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_clipEnds );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_tilling.x.ToString() + IOUtils.VECTOR_SEPARATOR + m_tilling.y.ToString() );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_useCurvature );
			IOUtils.AddFieldValueToString( ref nodeInfo, IOUtils.Vector2ToString( m_CurvatureVector ) );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_useTextureArray );
		}

		public override void Destroy()
		{
			base.Destroy();
			m_uvPort = null;
			m_texPort = null;
			m_scalePort = null;
			m_viewdirTanPort = null;
			m_pomUVPort = null;
		}
	}
}
