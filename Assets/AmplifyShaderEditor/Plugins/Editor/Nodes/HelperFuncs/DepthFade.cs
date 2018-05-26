// Amplify Shader Editor - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Depth Fade", "Surface Data", "Outputs a linear gradient representing the distance between the surface of this object and geometry behind" )]
	public sealed class DepthFade : ParentNode
	{
		private const string ConvertToLinearStr = "Convert To Linear";

		[SerializeField]
		private bool m_convertToLinear = true;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, "Distance" );
			m_inputPorts[ 0 ].FloatInternalData = 1;
			AddOutputPort( WirePortDataType.FLOAT, "Out" );
			m_useInternalPortData = true;
			m_autoWrapProperties = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				UIUtils.ShowNoVertexModeNodeMessage( this );
				return "0";
			}

			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return GetOutputColorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory ) );

			if( !( dataCollector.IsTemplate && dataCollector.IsLightweight ) )
				dataCollector.AddToIncludes( UniqueId, Constants.UnityCgLibFuncs );
			dataCollector.AddToUniforms( UniqueId, "uniform sampler2D _CameraDepthTexture;" );

			string screenPos = GeneratorUtils.GenerateScreenPosition( ref dataCollector, UniqueId, m_currentPrecisionType, !dataCollector.UsingCustomScreenPos );
			string screenPosNorm = GeneratorUtils.GenerateScreenPositionNormalized( ref dataCollector, UniqueId, m_currentPrecisionType, !dataCollector.UsingCustomScreenPos );

			string screenDepth = "UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(" + screenPos + ")))";
			if( dataCollector.IsTemplate && dataCollector.IsLightweight )
				screenDepth = "tex2Dproj( _CameraDepthTexture," + screenPos + ").r";

			if( m_convertToLinear )
			{
				if( dataCollector.IsTemplate && dataCollector.IsLightweight )
					screenDepth = string.Format( "LinearEyeDepth({0},_ZBufferParams)", screenDepth );
				else
					screenDepth = string.Format( "LinearEyeDepth({0})", screenDepth );
			}
			else
			{
				screenDepth = string.Format( "({0}*( _ProjectionParams.z - _ProjectionParams.y ))", screenDepth );
			}

			string distance = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );

			dataCollector.AddLocalVariable( UniqueId, "float screenDepth" + OutputId + " = " + screenDepth + ";" );
			if( dataCollector.IsTemplate && dataCollector.IsLightweight )
				dataCollector.AddLocalVariable( UniqueId, "float distanceDepth" + OutputId + " = abs( ( screenDepth" + OutputId + " - LinearEyeDepth( " + screenPosNorm + ".z,_ZBufferParams ) ) / ( " + distance + " ) );" );
			else
				dataCollector.AddLocalVariable( UniqueId, "float distanceDepth" + OutputId + " = abs( ( screenDepth" + OutputId + " - LinearEyeDepth( " + screenPosNorm + ".z ) ) / ( " + distance + " ) );" );

			m_outputPorts[ 0 ].SetLocalValue( "distanceDepth" + OutputId, dataCollector.PortCategory );
			return GetOutputColorItem( 0, outputId, "distanceDepth" + OutputId );
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			m_convertToLinear = EditorGUILayoutToggle( ConvertToLinearStr, m_convertToLinear );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() >= 13901 )
			{
				m_convertToLinear = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			}
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_convertToLinear );
		}
	}
}
