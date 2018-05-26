// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
//
// Custom Node Global Array
// Donated by Johann van Berkel

using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Global Array", "Constants And Properties", "The node returns a value from a global array, which you can configure by entering the name of the array in the node's settings.", null, KeyCode.None, true, false, null, null, "Johann van Berkel" )]
	public sealed class GlobalArrayNode : ParentNode
	{
		private const string DefaultArrayName = "MyGlobalArray";
		private const string TypeStr = "Type";
		private const string AutoRangeCheckStr = "Range Check";
		private const string ArrayFormatStr = "{0}[{1}]";

		private readonly string[] AvailableTypesLabel = { "Float", "Color", "Vector4", "Matrix4" };
		private readonly WirePortDataType[] AvailableTypesValues = { WirePortDataType.FLOAT, WirePortDataType.COLOR, WirePortDataType.FLOAT4, WirePortDataType.FLOAT4x4 };

		[SerializeField]
		private string m_name = DefaultArrayName;

		[SerializeField]
		private int m_index = 0;

		[SerializeField]
		private int m_arrayLength = 1;

		[SerializeField]
		private int m_type = 0;

		[SerializeField]
		private bool m_autoRangeCheck = false;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );

			AddInputPort( WirePortDataType.INT, false, "Index" );
			AddInputPort( WirePortDataType.INT, false, "Array Length" );

			AddOutputPort( WirePortDataType.FLOAT, "Out" );

			m_textLabelWidth = 95;
		}

		public override void DrawProperties()
		{
			EditorGUI.BeginChangeCheck();
			m_name = EditorGUILayoutStringField( "Name", m_name );
			if( EditorGUI.EndChangeCheck() )
			{
				m_name = UIUtils.RemoveInvalidCharacters( m_name );
				if( string.IsNullOrEmpty( m_name ) )
					m_name = DefaultArrayName;
			}

			if( !m_inputPorts[ 0 ].IsConnected )
			{
				EditorGUI.BeginChangeCheck();
				m_index = EditorGUILayoutIntField( m_inputPorts[ 0 ].Name, m_index );
				if( EditorGUI.EndChangeCheck() )
				{
					m_index = Mathf.Clamp( m_index, 0, ( m_arrayLength - 1 ) );
				}
			}

			if( !m_inputPorts[ 1 ].IsConnected )
			{
				EditorGUI.BeginChangeCheck();
				m_arrayLength = EditorGUILayoutIntField( m_inputPorts[ 1 ].Name, m_arrayLength );
				if( EditorGUI.EndChangeCheck() )
				{
					m_arrayLength = Mathf.Max( 1, m_arrayLength );
				}
			}
			EditorGUI.BeginChangeCheck();
			m_type = EditorGUILayoutPopup( TypeStr, m_type, AvailableTypesLabel );
			if( EditorGUI.EndChangeCheck() )
			{
				m_outputPorts[ 0 ].ChangeType( (WirePortDataType)AvailableTypesValues[ m_type ], false );
			}

			m_autoRangeCheck = EditorGUILayoutToggle( AutoRangeCheckStr, m_autoRangeCheck );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string arrayIndex = m_inputPorts[ 0 ].IsConnected ? m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector ) : m_index.ToString();
			string arrayLength = m_inputPorts[ 1 ].IsConnected ? m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector ) : m_arrayLength.ToString();

			string dataType = UIUtils.FinalPrecisionWirePortToCgType( m_currentPrecisionType, AvailableTypesValues[ m_type ] );
			dataCollector.AddToUniforms( UniqueId, dataType, string.Format( ArrayFormatStr, m_name, arrayLength ) );

			string index = m_autoRangeCheck ? string.Format( "clamp({0},0,({1} - 1))", arrayIndex, arrayLength ) : arrayIndex.ToString();
			m_outputPorts[ 0 ].SetLocalValue( string.Format( ArrayFormatStr, m_name, index ), dataCollector.PortCategory );

			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_name );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_index );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_arrayLength );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_type );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_autoRangeCheck );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_name = GetCurrentParam( ref nodeParams );
			m_index = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_arrayLength = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_type = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_autoRangeCheck = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
		}
	}
}
