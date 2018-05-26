using System;
using UnityEngine;


namespace AmplifyShaderEditor
{
	[Serializable]
	public class TemplateModulesHelper
	{
		[SerializeField]
		internal bool Foldout = false;

		private bool m_isDirty = false;

		[SerializeField]
		private TemplatesBlendModule m_blendOpHelper = new TemplatesBlendModule();

		[SerializeField]
		private TemplateCullModeModule m_cullModeHelper = new TemplateCullModeModule();

		[SerializeField]
		private TemplateColorMaskModule m_colorMaskHelper = new TemplateColorMaskModule();

		[SerializeField]
		private TemplatesStencilBufferModule m_stencilBufferHelper = new TemplatesStencilBufferModule();

		[SerializeField]
		private TemplateDepthModule m_depthOphelper = new TemplateDepthModule();

		[SerializeField]
		private TemplateTagsModule m_tagsHelper = new TemplateTagsModule();

		[SerializeField]
		private TemplateShaderModelModule m_shaderModelHelper = new TemplateShaderModelModule();

		[SerializeField]
		private TemplateAdditionalIncludesHelper m_additionalIncludes = new TemplateAdditionalIncludesHelper();

		[SerializeField]
		private TemplateAdditionalDefinesHelper m_additionalDefines = new TemplateAdditionalDefinesHelper();

		[SerializeField]
		private TemplateAdditionalPragmasHelper m_additionalPragmas = new TemplateAdditionalPragmasHelper();

		[SerializeField]
		private bool m_hasValidData = false;

		public void CopyFrom( TemplateModulesHelper other )
		{

			if( other.BlendOpHelper.IsDirty )
			{
				m_blendOpHelper.CopyFrom( other.BlendOpHelper );
			}

			if( other.CullModeHelper.IsDirty )
			{
				m_cullModeHelper.CopyFrom( other.CullModeHelper );
			}

			if( other.ColorMaskHelper.IsDirty )
			{
				m_colorMaskHelper.CopyFrom( other.ColorMaskHelper );
			}

			if( other.StencilBufferHelper.IsDirty )
			{
				m_stencilBufferHelper.CopyFrom( other.StencilBufferHelper );
			}

			if( other.DepthOphelper.IsDirty )
			{
				m_depthOphelper.CopyFrom( other.DepthOphelper );
			}

			if( other.TagsHelper.IsDirty )
			{
				m_tagsHelper.CopyFrom( other.TagsHelper );
			}

			if( other.ShaderModelHelper.IsDirty )
			{
				m_shaderModelHelper.CopyFrom( other.ShaderModelHelper );
			}
		}

		public void FetchDataFromTemplate( TemplateModulesData module )
		{
			if( module.PragmaTag.IsValid )
			{
				m_additionalPragmas.IsValid = true;
				m_additionalPragmas.FillNativeItems( module.IncludePragmaContainer.PragmasList );

				m_additionalIncludes.IsValid = true;
				m_additionalIncludes.FillNativeItems( module.IncludePragmaContainer.IncludesList );

				m_additionalDefines.IsValid = true;
				m_additionalDefines.FillNativeItems( module.IncludePragmaContainer.DefinesList );
			}
			else
			{
				m_additionalPragmas.IsValid = false;
				m_additionalIncludes.IsValid = false;
				m_additionalDefines.IsValid = false;
			}

			m_blendOpHelper.ConfigureFromTemplateData( module.BlendData );
			if( module.BlendData.DataCheck == TemplateDataCheck.Valid )
			{
				m_hasValidData = true;
			}

			m_cullModeHelper.ConfigureFromTemplateData( module.CullModeData );
			if( module.CullModeData.DataCheck == TemplateDataCheck.Valid )
			{
				m_hasValidData = true;
			}

			m_colorMaskHelper.ConfigureFromTemplateData( module.ColorMaskData );
			if( module.ColorMaskData.DataCheck == TemplateDataCheck.Valid )
			{
				m_hasValidData = true;
			}

			m_stencilBufferHelper.ConfigureFromTemplateData( module.StencilData );
			if( module.StencilData.DataCheck == TemplateDataCheck.Valid )
			{
				m_hasValidData = true;
			}

			m_depthOphelper.ConfigureFromTemplateData( module.DepthData );
			if( module.DepthData.DataCheck == TemplateDataCheck.Valid )
			{
				m_hasValidData = true;
			}

			m_tagsHelper.ConfigureFromTemplateData( module.TagData );
			if( module.TagData.DataCheck == TemplateDataCheck.Valid )
			{
				m_hasValidData = true;
			}
			
			m_shaderModelHelper.ConfigureFromTemplateData( module.ShaderModel );
			if( module.ShaderModel.DataCheck == TemplateDataCheck.Valid )
			{
				m_hasValidData = true;
			}
		}

		public void Draw( ParentNode owner, TemplateModulesData module )
		{

			switch( module.ShaderModel.DataCheck )
			{
				case TemplateDataCheck.Valid: m_shaderModelHelper.Draw( owner ); break;
				case TemplateDataCheck.Unreadable: m_shaderModelHelper.ShowUnreadableDataMessage(); break;
			}
			m_isDirty = m_shaderModelHelper.IsDirty;

			switch( module.CullModeData.DataCheck )
			{
				case TemplateDataCheck.Valid: m_cullModeHelper.Draw( owner ); break;
				case TemplateDataCheck.Unreadable: m_cullModeHelper.ShowUnreadableDataMessage(); break;
			}
			m_isDirty = m_isDirty || m_cullModeHelper.IsDirty;

			switch( module.ColorMaskData.DataCheck )
			{
				case TemplateDataCheck.Valid: m_colorMaskHelper.Draw( owner ); break;
				case TemplateDataCheck.Unreadable: m_colorMaskHelper.ShowUnreadableDataMessage(); break;
			}
			m_isDirty = m_isDirty || m_colorMaskHelper.IsDirty;

			switch( module.DepthData.DataCheck )
			{
				case TemplateDataCheck.Valid: m_depthOphelper.Draw( owner, false ); break;
				case TemplateDataCheck.Unreadable: m_depthOphelper.ShowUnreadableDataMessage(); break;
			}
			m_isDirty = m_isDirty || m_depthOphelper.IsDirty;

			switch( module.BlendData.DataCheck )
			{
				case TemplateDataCheck.Valid: m_blendOpHelper.Draw( owner, false ); break;
				case TemplateDataCheck.Unreadable: m_blendOpHelper.ShowUnreadableDataMessage(); break;
			}
			m_isDirty = m_isDirty || m_blendOpHelper.IsDirty;

			switch( module.StencilData.DataCheck )
			{
				case TemplateDataCheck.Valid:
				{
					CullMode cullMode = ( module.CullModeData.DataCheck == TemplateDataCheck.Valid ) ? m_cullModeHelper.CurrentCullMode : CullMode.Back;
					m_stencilBufferHelper.Draw( owner, cullMode, false );
				}
				break;
				case TemplateDataCheck.Unreadable:
				{
					m_stencilBufferHelper.ShowUnreadableDataMessage();
				}
				break;
			}
			m_isDirty = m_isDirty || m_stencilBufferHelper.IsDirty;

			switch( module.TagData.DataCheck )
			{
				case TemplateDataCheck.Valid: m_tagsHelper.Draw( owner, false ); break;
				case TemplateDataCheck.Unreadable: m_tagsHelper.ShowUnreadableDataMessage( owner ); break;
			}
			m_isDirty = m_isDirty || m_tagsHelper.IsDirty;

			if( module.PragmaTag.IsValid )
			{
				m_additionalDefines.Draw( owner );
				m_additionalIncludes.Draw( owner );
				m_additionalPragmas.Draw( owner );
			}

			m_isDirty = m_isDirty ||
						m_additionalDefines.IsDirty ||
						m_additionalIncludes.IsDirty ||
						m_additionalPragmas.IsDirty;
		}

		public void Destroy()
		{
			m_shaderModelHelper = null;
			m_blendOpHelper = null;
			m_cullModeHelper = null;
			m_colorMaskHelper.Destroy();
			m_colorMaskHelper = null;
			m_stencilBufferHelper.Destroy();
			m_stencilBufferHelper = null;
			m_tagsHelper.Destroy();
			m_tagsHelper = null;
			m_additionalDefines.Destroy();
			m_additionalDefines = null;
			m_additionalIncludes.Destroy();
			m_additionalIncludes = null;
			m_additionalPragmas.Destroy();
			m_additionalPragmas = null;
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			try
			{
				m_blendOpHelper.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_cullModeHelper.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_colorMaskHelper.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_stencilBufferHelper.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_depthOphelper.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_tagsHelper.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_shaderModelHelper.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_additionalDefines.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_additionalPragmas.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			try
			{
				m_additionalIncludes.ReadFromString( ref index, ref nodeParams );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void WriteToString( ref string nodeInfo )
		{
			m_blendOpHelper.WriteToString( ref nodeInfo );
			m_cullModeHelper.WriteToString( ref nodeInfo );
			m_colorMaskHelper.WriteToString( ref nodeInfo );
			m_stencilBufferHelper.WriteToString( ref nodeInfo );
			m_depthOphelper.WriteToString( ref nodeInfo );
			m_tagsHelper.WriteToString( ref nodeInfo );
			m_shaderModelHelper.WriteToString( ref nodeInfo );
			m_additionalDefines.WriteToString( ref nodeInfo );
			m_additionalPragmas.WriteToString( ref nodeInfo );
			m_additionalIncludes.WriteToString( ref nodeInfo );
		}

		public TemplatesBlendModule BlendOpHelper { get { return m_blendOpHelper; } }
		public TemplateCullModeModule CullModeHelper { get { return m_cullModeHelper; } }
		public TemplateColorMaskModule ColorMaskHelper { get { return m_colorMaskHelper; } }
		public TemplatesStencilBufferModule StencilBufferHelper { get { return m_stencilBufferHelper; } }
		public TemplateDepthModule DepthOphelper { get { return m_depthOphelper; } }
		public TemplateTagsModule TagsHelper { get { return m_tagsHelper; } }
		public TemplateShaderModelModule ShaderModelHelper { get { return m_shaderModelHelper; } }
		public TemplateAdditionalIncludesHelper AdditionalIncludes { get { return m_additionalIncludes; } }
		public TemplateAdditionalDefinesHelper AdditionalDefines { get { return m_additionalDefines; } }
		public TemplateAdditionalPragmasHelper AdditionalPragmas { get { return m_additionalPragmas; } }

		public bool HasValidData { get { return m_hasValidData; } }
		public bool IsDirty
		{
			get { return m_isDirty; }
			set
			{
				m_isDirty = value;
				if( !value )
				{
					m_blendOpHelper.IsDirty = false;
					m_cullModeHelper.IsDirty = false;
					m_colorMaskHelper.IsDirty = false;
					m_stencilBufferHelper.IsDirty = false;
					m_tagsHelper.IsDirty = false;
					m_shaderModelHelper.IsDirty = false;
					m_additionalDefines.IsDirty = false;
					m_additionalPragmas.IsDirty = false;
					m_additionalIncludes.IsDirty = false;
				}
			}
		}
		//	public bool Foldout { get { return m_foldout; } set { m_foldout = value;  } }
	}
}
