// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class TemplateInputData
	{
		public string PortName;
		public WirePortDataType DataType;
		public MasterNodePortCategory PortCategory;
		public int PortUniqueId;
		public int OrderId;
		public int TagGlobalStartIdx;
		public int TagLocalStartIdx;
		public string TagId;
		public string DefaultValue;
		public string LinkId;

		public TemplateInputData( int tagLocalStartIdx, int tagGlobalStartIdx, string tagId, string portName, string defaultValue, WirePortDataType dataType, MasterNodePortCategory portCategory, int portUniqueId, int orderId , string linkId )
		{
			DefaultValue = defaultValue;
			PortName = portName;
			DataType = dataType;
			PortCategory = portCategory;
			PortUniqueId = portUniqueId;
			OrderId = orderId;
			TagId = tagId;
			TagGlobalStartIdx = tagGlobalStartIdx;
			TagLocalStartIdx = tagLocalStartIdx;
			LinkId = linkId;
		}

		public TemplateInputData( TemplateInputData other )
		{
			DefaultValue = other.DefaultValue;
			PortName = other.PortName;
			DataType = other.DataType;
			PortCategory = other.PortCategory;
			PortUniqueId = other.PortUniqueId;
			OrderId = other.OrderId;
			TagId = other.TagId;
			TagGlobalStartIdx = other.TagGlobalStartIdx;
			LinkId = other.LinkId;
		}
	}

	[Serializable]
	public class TemplatePropertyContainer
	{
		[SerializeField]
		private List<TemplateProperty> m_propertyList = new List<TemplateProperty>();
		private Dictionary<string, TemplateProperty> m_propertyDict = new Dictionary<string, TemplateProperty>();


		public void AddId( TemplateProperty templateProperty )
		{
			m_propertyList.Add( templateProperty );
			m_propertyDict.Add( templateProperty.Id, templateProperty );
		}

		public void AddId( string body, string ID, bool searchIndentation = true )
		{
			AddId( body, ID, searchIndentation, string.Empty );
		}

		public void AddId( string body, string ID, bool searchIndentation, string customIndentation )
		{
			int propertyIndex = body.IndexOf( ID );
			if( propertyIndex > -1 )
			{
				if( searchIndentation )
				{
					int indentationIndex = -1;
					for( int i = propertyIndex; i > 0; i-- )
					{
						if( body[ i ] == TemplatesManager.TemplateNewLine )
						{
							indentationIndex = i + 1;
							break;
						}
					}
					if( indentationIndex > -1 )
					{
						int length = propertyIndex - indentationIndex;
						string indentation = ( length > 0 ) ? body.Substring( indentationIndex, length ) : string.Empty;
						TemplateProperty templateProperty = new TemplateProperty( ID, indentation, false );
						m_propertyList.Add( templateProperty );
						m_propertyDict.Add( templateProperty.Id, templateProperty );
					}
				}
				else
				{
					TemplateProperty templateProperty = new TemplateProperty( ID, customIndentation, true );
					m_propertyList.Add( templateProperty );
					m_propertyDict.Add( templateProperty.Id, templateProperty );
				}
			}
		}

	
		public void AddId( string body, string ID, int propertyIndex, bool searchIndentation )
		{
			AddId( body, ID, propertyIndex, searchIndentation, string.Empty );
		}

		public void AddId( string body, string ID, int propertyIndex, bool searchIndentation, string customIndentation )
		{
			if( propertyIndex > -1 )
			{
				if( searchIndentation )
				{
					int indentationIndex = -1;
					for( int i = propertyIndex; i > 0; i-- )
					{
						if( body[ i ] == TemplatesManager.TemplateNewLine )
						{
							indentationIndex = i + 1;
							break;
						}
					}
					if( indentationIndex > -1 )
					{
						int length = propertyIndex - indentationIndex;
						string indentation = ( length > 0 ) ? body.Substring( indentationIndex, length ) : string.Empty;
						TemplateProperty templateProperty = new TemplateProperty( ID, indentation, false );
						m_propertyList.Add( templateProperty );
						m_propertyDict.Add( templateProperty.Id, templateProperty );
					}
				}
				else
				{
					TemplateProperty templateProperty = new TemplateProperty( ID, customIndentation, true );
					m_propertyList.Add( templateProperty );
					m_propertyDict.Add( templateProperty.Id, templateProperty );
				}
			}

		}
		public void BuildInfo()
		{
			if( m_propertyDict == null )
			{
				m_propertyDict = new Dictionary<string, TemplateProperty>();
			}

			if( m_propertyList.Count != m_propertyDict.Count )
			{
				m_propertyDict.Clear();
				for( int i = 0; i < m_propertyList.Count; i++ )
				{
					m_propertyDict.Add( m_propertyList[ i ].Id, m_propertyList[ i ] );
				}
			}
		}

		public void ResetTemplateUsageData()
		{
			BuildInfo();
			for( int i = 0; i < m_propertyList.Count; i++ )
			{
				m_propertyList[ i ].Used = false;
			}
		}

		public void Reset()
		{
			m_propertyList.Clear();
			m_propertyDict.Clear();
		}

		public void Destroy()
		{
			m_propertyList.Clear();
			m_propertyList = null;
			m_propertyDict.Clear();
			m_propertyDict = null;
		}


		public Dictionary<string, TemplateProperty> PropertyDict { get { return m_propertyDict; } }
		public List<TemplateProperty> PropertyList { get { return m_propertyList; } }
	}

	[Serializable]
	public class TemplateProperty
	{
		public bool UseIndentationAtStart = false;
		public string Indentation;
		public bool UseCustomIndentation;
		public string Id;
		public bool AutoLineFeed;
		public bool Used;

		public TemplateProperty( string id, string indentation, bool useCustomIndentation )
		{
			Id = id;
			Indentation = indentation;
			UseCustomIndentation = useCustomIndentation;
			AutoLineFeed = !string.IsNullOrEmpty( indentation );
			Used = false;
		}
	}

	[Serializable]
	public class TemplateFunctionData
	{
		public int MainBodyLocalIdx;
		public string MainBodyName;

		public string Id;
		public int Position;
		public string InVarType;
		public string InVarName;
		public string OutVarType;
		public string OutVarName;
		public MasterNodePortCategory Category;
		public TemplateFunctionData( int mainBodyLocalIdx, string mainBodyName, string id, int position, string inVarInfo, string outVarInfo, MasterNodePortCategory category )
		{
			MainBodyLocalIdx = mainBodyLocalIdx;
			MainBodyName = mainBodyName;
			Id = id;
			Position = position;
			{
				string[] inVarInfoArr = inVarInfo.Split( IOUtils.VALUE_SEPARATOR );
				if( inVarInfoArr.Length > 1 )
				{
					InVarType = inVarInfoArr[ 1 ];
					InVarName = inVarInfoArr[ 0 ];
				}
			}
			{
				string[] outVarInfoArr = outVarInfo.Split( IOUtils.VALUE_SEPARATOR );
				if( outVarInfoArr.Length > 1 )
				{
					OutVarType = outVarInfoArr[ 1 ];
					OutVarName = outVarInfoArr[ 0 ];
				}
			}
			Category = category;
		}
	}

	[Serializable]
	public class TemplateTagData
	{
		public int StartIdx = -1;
		public string Id;
		public bool SearchIndentation;
		public string CustomIndentation;


		public TemplateTagData( int startIdx, string id, bool searchIndentation )
		{
			StartIdx = startIdx;
			Id = id;
			SearchIndentation = searchIndentation;
			CustomIndentation = string.Empty;
		}

		public TemplateTagData( string id, bool searchIndentation )
		{
			Id = id;
			SearchIndentation = searchIndentation;
			CustomIndentation = string.Empty;
		}

		public TemplateTagData( string id, bool searchIndentation, string customIndentation )
		{
			Id = id;
			SearchIndentation = searchIndentation;
			CustomIndentation = customIndentation;
		}

		public bool IsValid { get { return StartIdx >= 0; } }
	}

	public enum TemplatePortIds
	{
		Name = 0,
		DataType,
		UniqueId,
		OrderId,
		Link
	}

	public enum TemplateCommonTagId
	{
		Property = 0,
		Global = 1,
		Function = 2,
		Tag = 3,
		Pragmas = 4,
		Pass = 5,
		Params_Vert = 6,
		Params_Frag = 7
		//CullMode	= 8,
		//BlendMode   = 9,
		//BlendOp		= 10,
		//ColorMask	= 11,
		//StencilOp	= 12
	}

	public class TemplatesManager
	{
		public static int MPShaderVersion = 14503;
		public static bool Initialized = false;
		public static readonly string TemplateShaderNameBeginTag = "/*ase_name*/";

		//public static readonly string TemplateLocalVarTag = "/*ase_local_var*/";
		public static readonly string TemplateMPSubShaderTag = "\\bSubShader\\b\\s*{";
		public static readonly string TemplateMPPassTag = "\\bPass\\b\\s*{";
		public static readonly string TemplateLocalVarTag = "/*ase_local_var*/";
		public static readonly string TemplatePragmaTag = "/*ase_pragma*/";
		public static readonly string TemplatePassTag = "/*ase_pass*/";
		public static readonly string TemplatePropertyTag = "/*ase_props*/\n";
		public static readonly string TemplateGlobalsTag = "/*ase_globals*/\n";
		public static readonly string TemplateInterpolatorBeginTag = "/*ase_interp(";
		public static readonly string TemplateVertexDataTag = "/*ase_vdata:";

		public static readonly string TemplateExcludeFromGraphTag = "/*ase_hide_pass*/";
		public static readonly string TemplateMainPassTag = "/*ase_main_pass*/";

		public static readonly string TemplateFunctionsTag = "/*ase_functions*/\n";
		//public static readonly string TemplateTagsTag = "/*ase_tags*/";

		//public static readonly string TemplateCullModeTag = "/*ase_cull_mode*/";
		//public static readonly string TemplateBlendModeTag = "/*ase_blend_mode*/";
		//public static readonly string TemplateBlendOpTag = "/*ase_blend_op*/";
		//public static readonly string TemplateColorMaskTag = "/*ase_color_mask*/";
		//public static readonly string TemplateStencilOpTag = "/*ase_stencil*/";

		public static readonly string TemplateCodeSnippetAttribBegin = "#CODE_SNIPPET_ATTRIBS_BEGIN#";
		public static readonly string TemplateCodeSnippetAttribEnd = "#CODE_SNIPPET_ATTRIBS_END#\n";
		public static readonly string TemplateCodeSnippetEnd = "#CODE_SNIPPET_END#\n";

		public static readonly char TemplateNewLine = '\n';

		// INPUTS AREA
		public static readonly string TemplateInputsVertBeginTag = "/*ase_vert_out:";
		public static readonly string TemplateInputsFragBeginTag = "/*ase_frag_out:";
		public static readonly string TemplateInputsVertParamsTag = "/*ase_vert_input*/";
		public static readonly string TemplateInputsFragParamsTag = "/*ase_frag_input*/";


		// CODE AREA
		public static readonly string TemplateVertexCodeBeginArea = "/*ase_vert_code:";
		public static readonly string TemplateFragmentCodeBeginArea = "/*ase_frag_code:";


		public static readonly string TemplateEndOfLine = "*/\n";
		public static readonly string TemplateEndSectionTag = "*/";
		public static readonly string TemplateFullEndTag = "/*end*/";

		public static readonly string NameFormatter = "\"{0}\"";

		public static readonly TemplateTagData[] CommonTags = { new TemplateTagData( TemplatePropertyTag,true),
																new TemplateTagData( TemplateGlobalsTag,true),
																new TemplateTagData( TemplateFunctionsTag,true),
																//new TemplateTagData( TemplateTagsTag,false," "),
																new TemplateTagData( TemplatePragmaTag,true),
																new TemplateTagData( TemplatePassTag,true),
																new TemplateTagData( TemplateInputsVertParamsTag,false),
																new TemplateTagData( TemplateInputsFragParamsTag,false)
																//new TemplateTagData( TemplateCullModeTag,false),
																//new TemplateTagData( TemplateBlendModeTag,false),
																//new TemplateTagData( TemplateBlendOpTag,false),
																//new TemplateTagData( TemplateColorMaskTag,false),
																//new TemplateTagData( TemplateStencilOpTag,true),
																};



		private static Dictionary<string, TemplateDataParent> m_availableTemplates;
		private static List<TemplateDataParent> m_sortedTemplates;
		public static string[] AvailableTemplateNames;
		public static string CurrTemplateGUIDLoaded = string.Empty;
		public static bool IsTestTemplate { get { return CurrTemplateGUIDLoaded.Equals( "834460efe370abf4687f27dfa49b7f9c" ); } }
		public static Dictionary<string, string> OfficialTemplates = new Dictionary<string, string>()
		{
			{ "1976390536c6c564abb90fe41f6ee334","SRP/Lightweight"},
			{ "c71b220b631b6344493ea3cf87110c93","Single Pass/Post Process" },
			{ "6e114a916ca3e4b4bb51972669d463bf","Single Pass/Default Unlit" },
			{ "5056123faa0c79b47ab6ad7e8bf059a4","Single Pass/Default UI" },
			{ "0f8ba0101102bb14ebf021ddadce9b49","Single Pass/Default Sprites" },
			{ "0b6a9f8b4f707c74ca64c0be8e590de0","Single Pass/Particles Alpha Blended" },
			{ "e1de45c0d41f68c41b2cc20c8b9c05ef","Multi Pass/Unlit" }
		};

		public static void Init()
		{
			if( !Initialized )
			{
				m_availableTemplates = new Dictionary<string, TemplateDataParent>();
				m_sortedTemplates = new List<TemplateDataParent>();

				foreach( KeyValuePair<string, string> kvp in OfficialTemplates )
				{
					if( !string.IsNullOrEmpty( AssetDatabase.GUIDToAssetPath( kvp.Key ) ) )
					{
						AddTemplate( new TemplateMultiPass( kvp.Value, kvp.Key ) );
					}
				}

				//	AddTemplate( new TemplateMultiPass( "SRP/Lightweight", "1976390536c6c564abb90fe41f6ee334" ) );
				//	AddTemplate( new TemplateMultiPass( "Post Process", "c71b220b631b6344493ea3cf87110c93" ));
				//	AddTemplate( new TemplateMultiPass( "Default Unlit", "6e114a916ca3e4b4bb51972669d463bf" ));
				//	AddTemplate( new TemplateMultiPass( "Default UI", "5056123faa0c79b47ab6ad7e8bf059a4" ));
				//	AddTemplate( new TemplateMultiPass( "Default Sprites", "0f8ba0101102bb14ebf021ddadce9b49" ) );
				//	AddTemplate( new TemplateMultiPass( "Particles Alpha Blended", "0b6a9f8b4f707c74ca64c0be8e590de0" ));

				// Search for other possible templates on the project
				string[] allShaders = AssetDatabase.FindAssets( "t:shader" );
				for( int i = 0; i < allShaders.Length; i++ )
				{
					if( !m_availableTemplates.ContainsKey( allShaders[ i ] ) )
					{
						CheckAndLoadTemplate( allShaders[ i ] );
					}
				}

				// TODO: Sort list alphabeticaly 
				AvailableTemplateNames = new string[ m_sortedTemplates.Count + 1 ];
				AvailableTemplateNames[ 0 ] = "Custom";
				for( int i = 0; i < m_sortedTemplates.Count; i++ )
				{
					m_sortedTemplates[ i ].OrderId = i;
					AvailableTemplateNames[ i + 1 ] = m_sortedTemplates[ i ].Name;
				}
				Initialized = true;
			}

		}

		//[MenuItem( "Window/ASE Create Menu Items" )]
		public static void CreateTemplateMenuItems()
		{
			if( m_sortedTemplates == null || m_sortedTemplates.Count == 0 )
				return;

			System.Text.StringBuilder fileContents = new System.Text.StringBuilder();
			fileContents.Append( "// Amplify Shader Editor - Visual Shader Editing Tool\n" );
			fileContents.Append( "// Copyright (c) Amplify Creations, Lda <info@amplify.pt>\n" );
			fileContents.Append( "using UnityEditor;\n" );
			fileContents.Append( "\n" );
			fileContents.Append( "namespace AmplifyShaderEditor\n" );
			fileContents.Append( "{\n" );
			fileContents.Append( "\tpublic class TemplateMenuItems\n" );
			fileContents.Append( "\t{\n" );
			int fixedPriority = 85;
			for( int i = 0; i < m_sortedTemplates.Count; i++ )
			{
				fileContents.AppendFormat( "\t\t[ MenuItem( \"Assets/Create/Amplify Shader/{0}\", false, {1} )]\n", m_sortedTemplates[ i ].Name, fixedPriority );
				fileContents.AppendFormat( "\t\tpublic static void ApplyTemplate{0}()\n", i );
				fileContents.Append( "\t\t{\n" );
				fileContents.AppendFormat( "\t\t\tAmplifyShaderEditorWindow.CreateNewTemplateShader( \"{0}\" );\n", m_sortedTemplates[ i ].GUID );
				fileContents.Append( "\t\t}\n" );
			}
			fileContents.Append( "\t}\n" );
			fileContents.Append( "}\n" );
			string filePath = AssetDatabase.GUIDToAssetPath( "da0b931bd234a1e43b65f684d4b59bfb" );
			IOUtils.SaveTextfileToDisk( fileContents.ToString(), filePath, false );
			AssetDatabase.ImportAsset( filePath );
		}

		public static int GetIdForTemplate( TemplateData templateData )
		{
			if( templateData == null )
				return -1;

			for( int i = 0; i < m_sortedTemplates.Count; i++ )
			{
				if( m_sortedTemplates[ i ].GUID.Equals( templateData.GUID ) )
					return m_sortedTemplates[ i ].OrderId;
			}
			return -1;
		}



		public static void AddTemplate( TemplateDataParent templateData )
		{
			if( templateData == null || !templateData.IsValid )
				return;

			if( !m_availableTemplates.ContainsKey( templateData.GUID ) )
			{
				m_sortedTemplates.Add( templateData );
				m_availableTemplates.Add( templateData.GUID, templateData );
			}
		}

		public static void RemoveTemplate( string guid )
		{
				TemplateDataParent templateData = GetTemplate( guid );
				if( templateData != null )
				{
					RemoveTemplate( templateData );
				}
		}

		public static void RemoveTemplate( TemplateDataParent templateData )
		{
			if( m_availableTemplates != null )
				m_availableTemplates.Remove( templateData.GUID );

			m_sortedTemplates.Remove( templateData );
			templateData.Destroy();
		}

		public static void Destroy()
		{
			if( m_availableTemplates != null )
			{
				foreach( KeyValuePair<string, TemplateDataParent> kvp in m_availableTemplates )
				{
					kvp.Value.Destroy();
				}
				m_availableTemplates.Clear();
				m_availableTemplates = null;
			}
			AvailableTemplateNames = null;

			Initialized = false;
		}

		public static TemplateDataParent GetTemplate( int id )
		{
			if( id < m_sortedTemplates.Count )
				return m_sortedTemplates[ id ];

			return null;
		}

		public static TemplateDataParent GetTemplate( string guid )
		{
			if( m_availableTemplates == null && m_sortedTemplates != null )
			{
				m_availableTemplates = new Dictionary<string, TemplateDataParent>();
				for( int i = 0; i < m_sortedTemplates.Count; i++ )
				{
					m_availableTemplates.Add( m_sortedTemplates[ i ].GUID, m_sortedTemplates[ i ] );
				}
			}

			if( m_availableTemplates.ContainsKey( guid ) )
				return m_availableTemplates[ guid ];

			return null;
		}


		public static TemplateDataParent GetTemplateByName( string name )
		{
			if( m_availableTemplates == null && m_sortedTemplates != null )
			{
				m_availableTemplates = new Dictionary<string, TemplateDataParent>();
				for( int i = 0; i < m_sortedTemplates.Count; i++ )
				{
					m_availableTemplates.Add( m_sortedTemplates[ i ].GUID, m_sortedTemplates[ i ] );
				}
			}

			foreach( KeyValuePair<string, TemplateDataParent> kvp in m_availableTemplates )
			{
				if( kvp.Value.DefaultShaderName.Equals( name ) )
				{
					return kvp.Value;
				}
			}

			return null;
		}
		
		public static TemplateDataParent CheckAndLoadTemplate( string guid )
		{
			TemplateDataParent templateData = GetTemplate( guid );
			if( templateData == null )
			{
				string datapath = AssetDatabase.GUIDToAssetPath( guid );
				string body = IOUtils.LoadTextFileFromDisk( datapath );

				if( body.IndexOf( TemplatesManager.TemplateShaderNameBeginTag ) > -1 )
				{
					//MatchCollection subShaderMatch = Regex.Matches( body, TemplatesManager.TemplateMPSubShaderTag );
					//MatchCollection passMatch = Regex.Matches( body , TemplatesManager.TemplateMPPassTag );
					//if( subShaderMatch.Count > 1 || passMatch.Count > 1 )
					//{
					templateData = new TemplateMultiPass( string.Empty, guid );
					//}
					//else
					//{
					//	templateData = new TemplateData( string.Empty, guid, body );
					//}
					if( templateData.IsValid )
					{
						AddTemplate( templateData );
						return templateData;
					}
				}
			}
			return null;
		}

		
		public static int TemplateCount { get { return m_sortedTemplates.Count; } }
	}
}
