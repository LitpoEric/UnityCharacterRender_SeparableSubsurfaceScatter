using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	public class InlineProperty
	{
		[SerializeField]
		public int Value = 0;

		[SerializeField]
		public bool Active = false;

		[SerializeField]
		public int NodeId = -1;

		public InlineProperty() { }

		public InlineProperty( int val )
		{
			Value = val;
		}

		public void IntSlider( ref UndoParentNode owner, GUIContent content, int min, int max )
		{
			if( !Active )
			{
				EditorGUILayout.BeginHorizontal();
				Value = owner.EditorGUILayoutIntSlider( content, Value, min, max );
				if( GUILayout.Button( UIUtils.FloatIntIconON, UIUtils.FloatIntPickerONOFF, GUILayout.Width( 15 ), GUILayout.Height( 15 ) ) )
					Active = !Active;
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				DrawPicker( ref owner, content );
			}
		}

		public void EnumTypePopup( ref UndoParentNode owner, string content, string[] displayOptions )
		{
			if( !Active )
			{
				EditorGUILayout.BeginHorizontal();
				Value = owner.EditorGUILayoutPopup( content, Value, displayOptions );
				if( GUILayout.Button( UIUtils.FloatIntIconON, UIUtils.FloatIntPickerONOFF, GUILayout.Width( 15 ), GUILayout.Height( 15 ) ) )
					Active = !Active;
				EditorGUILayout.EndHorizontal();

			}
			else
			{
				DrawPicker( ref owner, content );
			}
		}

		public void CustomDrawer( ref UndoParentNode owner, DrawPropertySection Drawer, string content )
		{
			if( !Active )
			{

				EditorGUILayout.BeginHorizontal();
				Drawer( owner );
				if( GUILayout.Button( UIUtils.FloatIntIconON, UIUtils.FloatIntPickerONOFF, GUILayout.Width( 15 ), GUILayout.Height( 15 ) ) )
					Active = !Active;
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				DrawPicker( ref owner, content );
			}
		}

		public delegate void DrawPropertySection( UndoParentNode owner );

		private void DrawPicker( ref UndoParentNode owner, GUIContent content )
		{
			DrawPicker( ref owner, content.text );
		}

		private void DrawPicker( ref UndoParentNode owner, string content )
		{
			EditorGUILayout.BeginHorizontal();
			NodeId = owner.EditorGUILayoutIntPopup( content, NodeId, UIUtils.FloatIntNodeArr(), UIUtils.FloatIntNodeIds() );
			if( GUILayout.Button( UIUtils.FloatIntIconOFF, UIUtils.FloatIntPickerONOFF, GUILayout.Width( 15 ), GUILayout.Height( 15 ) ) )
				Active = !Active;
			EditorGUILayout.EndHorizontal();
		}

		public string GetValueOrProperty()
		{
			if( Active )
			{
				PropertyNode node = GetPropertyNode();
				if( node != null )
				{
					return "[" + node.PropertyName + "]";
				}
				else
				{
					Active = false;
					NodeId = -1;
					return Value.ToString();
				}
			}
			else
			{
				return Value.ToString();
			}
		}

		public string GetValueOrProperty( string defaultValue, bool parentesis = true )
		{
			if( Active )
			{
				PropertyNode node = GetPropertyNode();
				if( node != null )
				{
					if( parentesis )
						return "[" + node.PropertyName + "]";
					else
						return node.PropertyName;
				}
				else if( !string.IsNullOrEmpty( defaultValue ) )
				{
					Active = false;
					NodeId = -1;
					return defaultValue;
				}
				else
				{
					Active = false;
					NodeId = -1;
					return Value.ToString();
				}
			}
			else
			{
				return defaultValue;
			}
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			Value = Convert.ToInt32( nodeParams[ index++ ] );
			Active = Convert.ToBoolean( nodeParams[ index++ ] );
			NodeId = Convert.ToInt32( nodeParams[ index++ ] );
		}

		public void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, Value );
			IOUtils.AddFieldValueToString( ref nodeInfo, Active );
			IOUtils.AddFieldValueToString( ref nodeInfo, NodeId );
		}

		public void SetInlineNodeValue()
		{
			if( IsValid )
			{
				RangedFloatNode fnode = UIUtils.GetNode( NodeId ) as RangedFloatNode;
				if( fnode != null )
				{
					fnode.Value = Value;
					fnode.SetMaterialValueFromInline( Value );
				}
				else
				{
					IntNode inode = UIUtils.GetNode( NodeId ) as IntNode;
					inode.Value = Value;
					inode.SetMaterialValueFromInline( Value );
				}
			}
		}

		public bool IsValid
		{
			get
			{
				return Active && NodeId > -1;
			}
		}

		public PropertyNode GetPropertyNode()
		{
			return UIUtils.GetNode( NodeId ) as PropertyNode;
		}
	}
}
