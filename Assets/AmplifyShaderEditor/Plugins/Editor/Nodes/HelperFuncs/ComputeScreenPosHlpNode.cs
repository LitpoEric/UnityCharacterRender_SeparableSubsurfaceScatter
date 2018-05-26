// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Compute Screen Pos", "Camera And Screen", "Computes texture coordinate for doing a screenspace-mapped texture sample. Input is clip space position" )]
	public sealed class ComputeScreenPosHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "ComputeScreenPos";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].Name = "XYZW";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "computeScreenPos" + OutputId;
		}
	}
}
