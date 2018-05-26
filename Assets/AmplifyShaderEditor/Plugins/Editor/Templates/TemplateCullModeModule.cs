// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	public sealed class TemplateCullModeModule : TemplateModuleParent
	{
		private const string CullModeFormatStr = "Cull ";

		public TemplateCullModeModule() : base("Cull Mode"){ }

        private static readonly string CullModeStr = "Cull Mode";

		[SerializeField]
		private CullMode m_cullMode = CullMode.Back;

		public override void Draw( ParentNode owner, bool style = true )
		{
			EditorGUI.BeginChangeCheck();
			m_cullMode = (CullMode)owner.EditorGUILayoutEnumPopup( CullModeStr, m_cullMode );
			if( EditorGUI.EndChangeCheck() )
			{
				m_isDirty = true;
			}
		}

		public void CopyFrom( TemplateCullModeModule other )
		{
			m_cullMode = other.CurrentCullMode;
		}

		public override void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			bool validDataOnMeta = m_validData;
			if( UIUtils.CurrentShaderVersion() > TemplatesManager.MPShaderVersion )
			{
				validDataOnMeta = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( validDataOnMeta ) 
				m_cullMode = (CullMode)Enum.Parse( typeof( CullMode ), nodeParams[ index++ ] );
		}

		public override void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_validData );
			if( m_validData )
				IOUtils.AddFieldValueToString( ref nodeInfo, m_cullMode );
		}

		public override string GenerateShaderData()
		{
			return CullModeFormatStr + m_cullMode.ToString();
		}

		public void ConfigureFromTemplateData( TemplateCullModeData data )
		{
			bool newValidData = ( data.DataCheck == TemplateDataCheck.Valid );

			if( newValidData && m_validData != newValidData )
				m_cullMode = data.CullModeData;

			m_validData = newValidData;
		}

        public CullMode CurrentCullMode { get { return m_cullMode; } }
	}
}
