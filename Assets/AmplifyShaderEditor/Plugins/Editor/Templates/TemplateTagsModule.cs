using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class TemplateTagsModule : TemplateModuleParent
	{
		private const string CustomTagsStr = " SubShader Tags";
		private const string TagNameStr = "Name";
		private const string TagValueStr = "Value";

		private const float ShaderKeywordButtonLayoutWidth = 15;
		private ParentNode m_currentOwner;

		[SerializeField]
		private List<CustomTagData> m_availableTags = new List<CustomTagData>();

		private Dictionary<string, CustomTagData> m_availableTagsDict = new Dictionary<string, CustomTagData>();

		public TemplateTagsModule() : base( "SubShader Tags" ) { }

		public void CopyFrom( TemplateTagsModule other )
		{
			m_availableTags.Clear();
			m_availableTagsDict.Clear();

			int count = other.AvailableTags.Count;
			for( int i = 0; i < count; i++ )
			{
				CustomTagData newData = new CustomTagData( other.AvailableTags[ i ] );
				m_availableTags.Add( newData );
				m_availableTagsDict.Add( newData.TagName, newData );
			}
		}
		
		public void ConfigureFromTemplateData( TemplateTagsModuleData tagsData )
		{
			bool newValidData = tagsData.DataCheck == TemplateDataCheck.Valid;
			if( newValidData && newValidData != m_validData )
			{
				m_availableTags.Clear();
				m_availableTagsDict.Clear();
				int count = tagsData.Tags.Count;
				for( int i = 0; i < count; i++ )
				{
					CustomTagData tagData = new CustomTagData( tagsData.Tags[ i ].Name, tagsData.Tags[ i ].Value, i );
					m_availableTags.Add( tagData );
					m_availableTagsDict.Add( tagsData.Tags[ i ].Name, tagData );
				}
			}
			m_validData = newValidData;
		}

		public override void ShowUnreadableDataMessage( ParentNode owner )
		{
			NodeUtils.DrawPropertyGroup( ref m_foldoutValue, CustomTagsStr, base.ShowUnreadableDataMessage );
		}

		public override void Draw( ParentNode owner, bool style = true )
		{
			m_currentOwner = owner;
			
			if( style )
			{
				NodeUtils.DrawPropertyGroup( ref m_foldoutValue, CustomTagsStr, DrawMainBody, DrawButtons );
			}
			else
			{
				NodeUtils.DrawNestedPropertyGroup( ref m_foldoutValue, CustomTagsStr, DrawMainBody, DrawButtons );
			}
		}

		void DrawButtons()
		{
			EditorGUILayout.Separator();

			// Add tag
			if( GUILayout.Button( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
			{
				m_availableTags.Add( new CustomTagData() );
				EditorGUI.FocusTextInControl( null );
			}

			//Remove tag
			if( GUILayout.Button( string.Empty, UIUtils.MinusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
			{
				if( m_availableTags.Count > 0 )
				{
					m_availableTags.RemoveAt( m_availableTags.Count - 1 );
					EditorGUI.FocusTextInControl( null );
				}
			}
		}

		void DrawMainBody()
		{

			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.Separator();
				int itemCount = m_availableTags.Count;

				if( itemCount == 0 )
				{
					EditorGUILayout.HelpBox( "Your list is Empty!\nUse the plus button to add one.", MessageType.Info );
				}

				int markedToDelete = -1;
				float originalLabelWidth = EditorGUIUtility.labelWidth;
				for( int i = 0; i < itemCount; i++ )
				{
					m_availableTags[ i ].TagFoldout = m_currentOwner.EditorGUILayoutFoldout( m_availableTags[ i ].TagFoldout, string.Format( "[{0}] - {1}", i, m_availableTags[ i ].TagName ) );
					if( m_availableTags[ i ].TagFoldout )
					{
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 70;
						//Tag Name
						EditorGUI.BeginChangeCheck();
						m_availableTags[ i ].TagName = EditorGUILayout.TextField( TagNameStr, m_availableTags[ i ].TagName );
						if( EditorGUI.EndChangeCheck() )
						{
							m_availableTags[ i ].TagName = UIUtils.RemoveShaderInvalidCharacters( m_availableTags[ i ].TagName );
						}

						//Tag Value
						EditorGUI.BeginChangeCheck();
						m_availableTags[ i ].TagValue = EditorGUILayout.TextField( TagValueStr, m_availableTags[ i ].TagValue );
						if( EditorGUI.EndChangeCheck() )
						{
							m_availableTags[ i ].TagValue = UIUtils.RemoveShaderInvalidCharacters( m_availableTags[ i ].TagValue );
						}

						EditorGUIUtility.labelWidth = originalLabelWidth;

						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.Label( " " );
							// Add new port
							if( m_currentOwner.GUILayoutButton( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
							{
								m_availableTags.Insert( i + 1, new CustomTagData() );
								EditorGUI.FocusTextInControl( null );
							}

							//Remove port
							if( m_currentOwner.GUILayoutButton( string.Empty, UIUtils.MinusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
							{
								markedToDelete = i;
							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUI.indentLevel -= 1;
					}

				}
				if( markedToDelete > -1 )
				{
					if( m_availableTags.Count > markedToDelete )
					{
						m_availableTags.RemoveAt( markedToDelete );
						EditorGUI.FocusTextInControl( null );
					}
				}
				EditorGUILayout.Separator();
			}
			if( EditorGUI.EndChangeCheck() )
			{
				m_isDirty = true;
			}
		}

		void AddTagFromRead( string data )
		{
			string[] arr = data.Split( IOUtils.VALUE_SEPARATOR );
			if( arr.Length > 1 )
			{
				string name = arr[ 0 ];
				string value = arr[ 1 ];

				if( !m_availableTagsDict.ContainsKey( name ) )
				{
					CustomTagData tagData = new CustomTagData( name, value, m_availableTags.Count - 1 );
					m_availableTags.Add( tagData );
					m_availableTagsDict.Add( name, tagData );
				}
				else
				{
					if( m_availableTagsDict[ name ].TagId > -1 &&
						m_availableTagsDict[ name ].TagId < m_availableTags.Count )
					{
						m_availableTags[ m_availableTagsDict[ name ].TagId ].TagValue = value;
					}
					else
					{
						int count = m_availableTags.Count;
						for( int i = 0; i < count; i++ )
						{
							if( m_availableTags[ i ].TagName.Equals( name ) )
							{
								m_availableTags[ i ].TagValue = value;
							}
						}
					}
				}
			}
		}

		public override void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			bool validDataOnMeta = m_validData;
			if( UIUtils.CurrentShaderVersion() > TemplatesManager.MPShaderVersion )
			{
				validDataOnMeta = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( validDataOnMeta )
			{
				int count = Convert.ToInt32( nodeParams[ index++ ] );
				for( int i = 0; i < count; i++ )
				{
					AddTagFromRead( nodeParams[ index++ ] );
				}
			}
		}

		public override void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_validData );
			if( m_validData )
			{
				int tagsCount = m_availableTags.Count;
				IOUtils.AddFieldValueToString( ref nodeInfo, tagsCount );
				for( int i = 0; i < tagsCount; i++ )
				{
					IOUtils.AddFieldValueToString( ref nodeInfo, m_availableTags[ i ].ToString() );
				}
			}
		}

		public string GenerateTags()
		{
			int tagsCount = m_availableTags.Count;
			if( tagsCount == 0 )
				return string.Empty;

			string result = "Tags { ";

			for( int i = 0; i < tagsCount; i++ )
			{
				if( m_availableTags[ i ].IsValid )
				{
					result += m_availableTags[ i ].GenerateTag();
					if( i < tagsCount - 1 )
					{
						result += " ";
					}
				}
			}

			result += " }";

			return result;
		}

		public override void Destroy()
		{
			m_availableTags.Clear();
			m_availableTags = null;
			m_currentOwner = null;
			m_availableTagsDict.Clear();
			m_availableTagsDict = null;
		}
		public List<CustomTagData> AvailableTags { get { return m_availableTags; } }
	}
}
