// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public sealed class TemplateDepthModule : TemplateModuleParent
	{
		public TemplateDepthModule() : base( "Depth" ) { }

		[SerializeField]
		private bool m_validZTest = false;

		[SerializeField]
		private int m_zTestMode = 0;

		[SerializeField]
		private bool m_validZWrite = false;

		[SerializeField]
		private int m_zWriteMode = 0;

		[SerializeField]
		private float m_offsetFactor = 0;

		[SerializeField]
		private float m_offsetUnits = 0;

		[SerializeField]
		private bool m_offsetEnabled = false;

		[SerializeField]
		private bool m_validOffset = false;

		public void CopyFrom( TemplateDepthModule other )
		{
			m_validZTest = other.ValidZTest;
			m_validZWrite = other.ValidZWrite;
			m_validOffset = other.ValidOffset;
			m_zTestMode = other.ZTestMode;
			m_zWriteMode = other.ZWriteMode;
			m_offsetFactor = other.OffsetFactor;
			m_offsetUnits = other.OffsetUnits;
			m_offsetEnabled = other.OffsetEnabled;
		}

		public override void ShowUnreadableDataMessage( ParentNode owner )
		{
			bool foldoutValue = owner.ContainerGraph.ParentWindow.ExpandedDepth;
			NodeUtils.DrawPropertyGroup( ref foldoutValue, ZBufferOpHelper.DepthParametersStr, base.ShowUnreadableDataMessage );
			owner.ContainerGraph.ParentWindow.ExpandedDepth = foldoutValue;
		}

		public override void Draw( ParentNode owner , bool style = true)
		{
			if( style )
			{
				NodeUtils.DrawPropertyGroup( ref m_foldoutValue, ZBufferOpHelper.DepthParametersStr, () =>
				{
					EditorGUI.indentLevel++;
					DrawBlock( owner );
					EditorGUI.indentLevel--;
				} );
			}
			else
			{
				NodeUtils.DrawNestedPropertyGroup( ref m_foldoutValue, ZBufferOpHelper.DepthParametersStr, () =>
				{
					DrawBlock( owner );
				} );
			}
		}

		void DrawBlock(ParentNode owner)
		{
			EditorGUI.BeginChangeCheck();
			Color cachedColor = GUI.color;
			GUI.color = new Color( cachedColor.r, cachedColor.g, cachedColor.b, ( EditorGUIUtility.isProSkin ? 0.5f : 0.25f ) );
			//EditorGUILayout.BeginVertical( UIUtils.MenuItemBackgroundStyle );
			GUI.color = cachedColor;
			
			EditorGUILayout.Separator();

			if( m_validZWrite )
				m_zWriteMode = owner.EditorGUILayoutPopup( ZBufferOpHelper.ZWriteModeStr, m_zWriteMode, ZBufferOpHelper.ZWriteModeValues );

			if( m_validZTest )
				m_zTestMode = owner.EditorGUILayoutPopup( ZBufferOpHelper.ZTestModeStr, m_zTestMode, ZBufferOpHelper.ZTestModeLabels );


			if( m_validOffset )
			{
				m_offsetEnabled = owner.EditorGUILayoutToggle( ZBufferOpHelper.OffsetStr, m_offsetEnabled );
				if( m_offsetEnabled )
				{
					EditorGUI.indentLevel++;
					m_offsetFactor = owner.EditorGUILayoutFloatField( ZBufferOpHelper.OffsetFactorStr, m_offsetFactor );
					m_offsetUnits = owner.EditorGUILayoutFloatField( ZBufferOpHelper.OffsetUnitsStr, m_offsetUnits );
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.Separator();
			
			//EditorGUILayout.EndVertical();
			if( EditorGUI.EndChangeCheck() )
			{
				m_isDirty = true;
			}
		}

		public void ConfigureFromTemplateData( TemplateDepthData depthData )
		{
			if( depthData.ValidZTest && m_validZTest != depthData.ValidZTest )
			{
				m_zTestMode = ZBufferOpHelper.ZTestModeDict[ depthData.ZTestModeValue ];
			}

			if( depthData.ValidZWrite && m_validZWrite != depthData.ValidZWrite )
			{
				m_zWriteMode = ZBufferOpHelper.ZWriteModeDict[ depthData.ZWriteModeValue ];
			}
			
			if( depthData.ValidOffset && m_validOffset != depthData.ValidOffset )
			{
				m_offsetFactor = depthData.OffsetFactor;
				m_offsetUnits = depthData.OffsetUnits;
			}

			m_validZTest = depthData.ValidZTest;
			m_validZWrite = depthData.ValidZWrite;
			m_offsetEnabled = depthData.ValidOffset;
			m_validOffset = depthData.ValidOffset;
			m_validData = m_validZTest || m_validZWrite || m_validOffset;
		}

		public void ReadZWriteFromString( ref uint index, ref string[] nodeParams )
		{
			bool validDataOnMeta = m_validZWrite;
			if( UIUtils.CurrentShaderVersion() > TemplatesManager.MPShaderVersion )
			{
				validDataOnMeta = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( validDataOnMeta )
				m_zWriteMode = Convert.ToInt32( nodeParams[ index++ ] );
		}

		public void ReadZTestFromString( ref uint index, ref string[] nodeParams )
		{
			bool validDataOnMeta = m_validZTest;
			if( UIUtils.CurrentShaderVersion() > TemplatesManager.MPShaderVersion )
			{
				validDataOnMeta = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( validDataOnMeta )
				m_zTestMode = Convert.ToInt32( nodeParams[ index++ ] );
		}

		public void ReadOffsetFromString( ref uint index, ref string[] nodeParams )
		{
			bool validDataOnMeta = m_validOffset;
			if( UIUtils.CurrentShaderVersion() > TemplatesManager.MPShaderVersion )
			{
				validDataOnMeta = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( validDataOnMeta )
			{
				m_offsetEnabled = Convert.ToBoolean( nodeParams[ index++ ] );
				m_offsetFactor = Convert.ToSingle( nodeParams[ index++ ] );
				m_offsetUnits = Convert.ToSingle( nodeParams[ index++ ] );
			}
		}
		
		public override void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			ReadZWriteFromString( ref index, ref nodeParams );
			ReadZTestFromString( ref index, ref nodeParams );
			ReadOffsetFromString( ref index, ref nodeParams );
		}

		public void WriteZWriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_validZWrite );
			if( m_validZWrite )
				IOUtils.AddFieldValueToString( ref nodeInfo, m_zWriteMode );
		}

		public void WriteZTestToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_validZTest );
			if( m_validZTest )
				IOUtils.AddFieldValueToString( ref nodeInfo, m_zTestMode );
		}

		public void WriteOffsetToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_validOffset );
			if( m_validOffset )
			{
				IOUtils.AddFieldValueToString( ref nodeInfo, m_offsetEnabled );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_offsetFactor );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_offsetUnits );
			}
		}

		public override void WriteToString( ref string nodeInfo )
		{
			WriteZWriteToString( ref nodeInfo );
			WriteZTestToString( ref nodeInfo );
			WriteOffsetToString( ref nodeInfo );
		}

		public bool IsActive { get { return m_zTestMode != 0 || m_zWriteMode != 0 || m_offsetEnabled; } }
		public string CurrentZWriteMode
		{
			get
			{
				int finalZWrite = ( m_zWriteMode == 0 ) ? 1 : m_zWriteMode;
				return  "ZWrite " + ZBufferOpHelper.ZWriteModeValues[ finalZWrite ]  + "\n";
			}
		}
		public string CurrentZTestMode
		{
			get
			{
				int finalZTestMode = ( m_zTestMode == 0 )?3 : m_zTestMode;
				return "ZTest " + ZBufferOpHelper.ZTestModeValues[ finalZTestMode ] + "\n";
			}
		}
		public string CurrentOffset
		{
			get
			{
				if( m_offsetEnabled )
					return "Offset " + m_offsetFactor + " , " + m_offsetUnits + "\n";
				else
					return "Offset 0,0\n";
			}
		}
		public bool ValidZTest { get { return m_validZTest; } }
		public bool ValidZWrite { get { return m_validZWrite; } }
		public bool ValidOffset { get { return m_validOffset; } }
		public int ZTestMode { get { return m_zTestMode; } }
		public int ZWriteMode { get { return m_zWriteMode; } }
		public float OffsetFactor { get { return m_offsetFactor; } }
		public float OffsetUnits { get { return m_offsetUnits; } }
		public bool OffsetEnabled { get { return m_offsetEnabled; } }

	}
}
