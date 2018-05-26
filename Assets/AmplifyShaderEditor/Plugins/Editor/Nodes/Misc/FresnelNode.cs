// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
// http://kylehalladay.com/blog/tutorial/2014/02/18/Fresnel-Shaders-From-The-Ground-Up.html
// http://http.developer.nvidia.com/CgTutorial/cg_tutorial_chapter07.html

using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Fresnel", "Surface Data", "Simple Fresnel effect" )]
	public sealed class FresnelNode : ParentNode
	{
		private const string FresnedFinalVar = "fresnelNode";

		[SerializeField]
		private ViewSpace m_normalSpace = ViewSpace.Tangent;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT3, false, "Normal" );
			AddInputPort( WirePortDataType.FLOAT, false, "Bias" );
			AddInputPort( WirePortDataType.FLOAT, false, "Scale" );
			AddInputPort( WirePortDataType.FLOAT, false, "Power" );
			AddOutputPort( WirePortDataType.FLOAT, "Out" );
			m_inputPorts[ 1 ].AutoDrawInternalData = true;
			m_inputPorts[ 2 ].AutoDrawInternalData = true;
			m_inputPorts[ 3 ].AutoDrawInternalData = true;
			m_autoWrapProperties = true;
			m_drawPreviewAsSphere = true;
			m_inputPorts[ 0 ].Vector3InternalData = Vector3.forward;
			m_inputPorts[ 2 ].FloatInternalData = 1;
			m_inputPorts[ 3 ].FloatInternalData = 5;
			m_previewShaderGUID = "240145eb70cf79f428015012559f4e7d";
		}

		public override void SetPreviewInputs()
		{
			base.SetPreviewInputs();

			if( m_normalSpace == ViewSpace.Tangent && m_inputPorts[ 0 ].IsConnected )
				m_previewMaterialPassId = 2;
			else if( m_normalSpace == ViewSpace.World && m_inputPorts[ 0 ].IsConnected )
				m_previewMaterialPassId = 1;
			else
				m_previewMaterialPassId = 0;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();

			EditorGUI.BeginChangeCheck();
			m_normalSpace = (ViewSpace)EditorGUILayoutEnumPopup( "Normal Space", m_normalSpace );
			if( EditorGUI.EndChangeCheck() )
			{
				UpdatePort();
			}

			if( !m_inputPorts[ 1 ].IsConnected )
				m_inputPorts[ 1 ].FloatInternalData = EditorGUILayoutFloatField( m_inputPorts[ 1 ].Name, m_inputPorts[ 1 ].FloatInternalData );
			if( !m_inputPorts[ 2 ].IsConnected )
				m_inputPorts[ 2 ].FloatInternalData = EditorGUILayoutFloatField( m_inputPorts[ 2 ].Name, m_inputPorts[ 2 ].FloatInternalData );
			if( !m_inputPorts[ 3 ].IsConnected )
				m_inputPorts[ 3 ].FloatInternalData = EditorGUILayoutFloatField( m_inputPorts[ 3 ].Name, m_inputPorts[ 3 ].FloatInternalData );
		}

		private void UpdatePort()
		{
			if( m_normalSpace == ViewSpace.World )
				m_inputPorts[ 0 ].Name = "World Normal";
			else
				m_inputPorts[ 0 ].Name = "Normal";

			m_sizeIsDirty = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			if( dataCollector.IsFragmentCategory )
				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_POS );

			string viewdir = GeneratorUtils.GenerateViewDirection( ref dataCollector, UniqueId, ViewSpace.World );

			string normal = string.Empty;
			if( m_inputPorts[ 0 ].IsConnected )
			{
				normal = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );

				if( dataCollector.IsFragmentCategory )
				{
					dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );

					if( m_normalSpace == ViewSpace.Tangent )
					{
						if( dataCollector.IsTemplate )
						{
							normal = dataCollector.TemplateDataCollectorInstance.GetWorldNormal( UniqueId, m_currentPrecisionType, normal, OutputId );
						} else
						{
							normal = "WorldNormalVector( " + Constants.InputVarStr + " , " + normal + " )";
						}
						dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, m_currentPrecisionType );
						dataCollector.ForceNormal = true;

					}
				}
				else
				{
					if( m_normalSpace == ViewSpace.Tangent )
					{
						string wtMatrix = GeneratorUtils.GenerateWorldToTangentMatrix( ref dataCollector, UniqueId, m_currentPrecisionType );
						normal = "mul( " + normal + "," + wtMatrix + " )";
					}
				}
			}
			else
			{
				if( dataCollector.IsFragmentCategory )
				{
					dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, m_currentPrecisionType );
					if( dataCollector.DirtyNormal )
						dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );
				}

				normal = dataCollector.IsTemplate ? dataCollector.TemplateDataCollectorInstance.GetWorldNormal( m_currentPrecisionType ) : GeneratorUtils.GenerateWorldNormal( ref dataCollector, UniqueId );
				normal = string.Format( "normalize( {0} )", normal );
				if( dataCollector.DirtyNormal )
					dataCollector.ForceNormal = true;
			}

			string bias = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );
			string scale = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );
			string power = m_inputPorts[ 3 ].GeneratePortInstructions( ref dataCollector );

			string fresnelNDotVLocalValue = "dot( " + normal + ", " + viewdir + " )";
			string fresnelNDotVLocalVar = "fresnelNDotV" + OutputId;
			dataCollector.AddLocalVariable( UniqueId, m_currentPrecisionType, WirePortDataType.FLOAT, fresnelNDotVLocalVar, fresnelNDotVLocalValue );

			string fresnelFinalVar = FresnedFinalVar + OutputId;
			string result = string.Format( "( {0} + {1} * pow( 1.0 - {2}, {3} ) )", bias, scale, fresnelNDotVLocalVar, power );

			RegisterLocalVariable( 0, result, ref dataCollector, fresnelFinalVar );
			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
		
		public override void PropagateNodeData( NodeData nodeData, ref MasterNodeDataCollector dataCollector )
		{
			base.PropagateNodeData( nodeData, ref dataCollector );
			if( m_normalSpace == ViewSpace.Tangent && m_inputPorts[0].IsConnected )
				dataCollector.DirtyNormal = true;
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() >= 13202 )
			{
				m_normalSpace = (ViewSpace)Enum.Parse( typeof( ViewSpace ), GetCurrentParam( ref nodeParams ) );
			}
			else
			{
				m_normalSpace = ViewSpace.World;
			}
			UpdatePort();
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_normalSpace );
		}

	}
}
