using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class CustomTagData
	{
		private const string TagFormat = "\"{0}\"=\"{1}\"";
		public string TagName;
		public string TagValue;
		public int TagId = -1;
		public bool TagFoldout = true;

		public CustomTagData()
		{
			TagName = string.Empty;
			TagValue = string.Empty;
		}

		public CustomTagData( CustomTagData other )
		{
			TagName = other.TagName;
			TagValue = other.TagValue;
			TagId = other.TagId;
			TagFoldout = other.TagFoldout;
		}
		
		public CustomTagData( string name, string value , int id )
		{
			TagName = name;
			TagValue = value;
			TagId = id;
		}

		public CustomTagData( string data )
		{
			string[] arr = data.Split( IOUtils.VALUE_SEPARATOR );
			if( arr.Length > 1 )
			{
				TagName = arr[ 0 ];
				TagValue = arr[ 1 ];
			}
		}

		public override string ToString()
		{
			return TagName + IOUtils.VALUE_SEPARATOR + TagValue;
		}

		public string GenerateTag()
		{
			return string.Format( TagFormat, TagName, TagValue );
		}

		public bool IsValid { get { return ( !string.IsNullOrEmpty( TagValue ) && !string.IsNullOrEmpty( TagName ) ); } }
	}

	[Serializable]
	public class CustomTagsHelper
	{
		private const string CustomTagsStr = " Custom SubShader Tags";
		private const string TagNameStr = "Name";
		private const string TagValueStr = "Value";

		private const float ShaderKeywordButtonLayoutWidth = 15;
		private ParentNode m_currentOwner;

		[SerializeField]
		private List<CustomTagData> m_availableTags = new List<CustomTagData>();

		public void Draw( ParentNode owner )
		{
			m_currentOwner = owner;
			bool value = EditorVariablesManager.ExpandedCustomTags.Value;
			NodeUtils.DrawPropertyGroup( ref value, CustomTagsStr, DrawMainBody, DrawButtons );
			EditorVariablesManager.ExpandedCustomTags.Value = value;
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


		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			int count = Convert.ToInt32( nodeParams[ index++ ] );
			for( int i = 0; i < count; i++ )
			{
				m_availableTags.Add( new CustomTagData( nodeParams[ index++ ] ) );
			}
		}

		public void WriteToString( ref string nodeInfo )
		{
			int tagsCount = m_availableTags.Count;
			IOUtils.AddFieldValueToString( ref nodeInfo, tagsCount );
			for( int i = 0; i < tagsCount; i++ )
			{
				IOUtils.AddFieldValueToString( ref nodeInfo, m_availableTags[ i ].ToString() );
			}
		}

		public string GenerateCustomTags()
		{
			int tagsCount = m_availableTags.Count;
			string result = tagsCount == 0 ? string.Empty : " ";

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
			return result;
		}

		public void Destroy()
		{
			m_availableTags.Clear();
			m_availableTags = null;
			m_currentOwner = null;
		}
	}
}
