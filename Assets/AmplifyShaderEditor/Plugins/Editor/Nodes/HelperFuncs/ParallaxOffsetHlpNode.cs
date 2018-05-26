// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Parallax Offset", "UV Coordinates", "Calculates UV offset for parallax normal mapping" )]
	public sealed class ParallaxOffsetHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "ParallaxOffset";
			m_inputPorts[ 0 ].ChangeProperties( "H", WirePortDataType.FLOAT, false );
			AddInputPort( WirePortDataType.FLOAT, false, "Height" );
			AddInputPort( WirePortDataType.FLOAT3, false, "ViewDir" );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT2, false );
			m_outputPorts[ 0 ].Name = "Out";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "paralaxOffset" + OutputId;
		}
	}
}
