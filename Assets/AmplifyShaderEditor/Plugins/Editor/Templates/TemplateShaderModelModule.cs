// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public sealed class TemplateShaderModelModule : TemplateModuleParent
	{
		private const string ShaderModelStr = "Shader Model";
		private const string ShaderModelFormatStr = "#pragma target ";

		[SerializeField]
		private int m_shaderModelIdx = 2;

		public TemplateShaderModelModule() : base("Shader Model"){ }
		
		public override void Draw( ParentNode owner, bool style = true )
		{
			EditorGUI.BeginChangeCheck();
			m_shaderModelIdx = owner.EditorGUILayoutPopup( ShaderModelStr, m_shaderModelIdx, TemplateHelperFunctions.AvailableShaderModels );
			if( EditorGUI.EndChangeCheck() )
			{
				m_isDirty = true;
			}
		}

		public void CopyFrom( TemplateShaderModelModule other )
		{
			m_shaderModelIdx = other.CurrentShaderModel;
		}

		public override void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			bool validDataOnMeta = m_validData;
			if( UIUtils.CurrentShaderVersion() > TemplatesManager.MPShaderVersion )
			{
				validDataOnMeta = Convert.ToBoolean( nodeParams[ index++ ] );
			}
			
			if( validDataOnMeta )
				m_shaderModelIdx = Convert.ToInt32( nodeParams[ index++ ] );
		}

		public override void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_validData );
			if( m_validData )
				IOUtils.AddFieldValueToString( ref nodeInfo, m_shaderModelIdx );
		}

		public override string GenerateShaderData()
		{
			return ShaderModelFormatStr + TemplateHelperFunctions.AvailableShaderModels[ m_shaderModelIdx ];
		}

		public void ConfigureFromTemplateData( TemplateShaderModelData data )
		{
			bool newValidData = ( data.DataCheck == TemplateDataCheck.Valid );

			if( newValidData && m_validData != newValidData )
			{
				if( TemplateHelperFunctions.ShaderModelToArrayIdx.ContainsKey( data.Value ) )
				{
					m_shaderModelIdx = TemplateHelperFunctions.ShaderModelToArrayIdx[ data.Value ];
				}
			}

			m_validData = newValidData;
		}

		public int CurrentShaderModel { get { return m_shaderModelIdx; } }
		public int InterpolatorAmount
		{
			get
			{
				return TemplateHelperFunctions.AvailableInterpolators[ TemplateHelperFunctions.AvailableShaderModels[ m_shaderModelIdx ] ];
			}
		}
		
	}
}
