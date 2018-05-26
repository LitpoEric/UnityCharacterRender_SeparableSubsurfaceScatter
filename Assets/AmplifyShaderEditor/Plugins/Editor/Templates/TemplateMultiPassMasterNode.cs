// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
#define SHOW_TEMPLATE_HELP_BOX

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Template Master Node", "Master", "Shader Generated according to template rules", null, KeyCode.None, false )]
	public sealed class TemplateMultiPassMasterNode : MasterNode
	{
		private const string SubTitleFormatterStr = "(SubShader {0} Pass {1})";
		private const string NoSubShaderPropertyStr = "No Sub-Shader properties available";
		private const string NoPassPropertyStr = "No Pass properties available";

		private const string WarningMessage = "Templates is a feature that is still heavily under development and users may experience some problems.\nPlease email support@amplify.pt if any issue occurs.";
		private const string OpenTemplateStr = "Edit Template";
		private const string CommonPropertiesStr = "Common Properties ";
		private const string SubShaderModuleStr = "SubShader ";
		private const string PassModuleStr = "Pass ";

		private const string PassNameStr = "Name";
		private const string PassNameFormateStr = "Name \"{0}\"";

		private bool m_reRegisterTemplateData = false;
		private bool m_fireTemplateChange = false;
		private bool m_fetchMasterNodeCategory = false;

		[SerializeField]
		private string m_templateGUID = "4e1801f860093ba4f9eb58a4b556825b";

		[SerializeField]
		private int m_passIdx = 0;

		[SerializeField]
		private string m_passIdxStr = string.Empty;

		[SerializeField]
		private bool m_passFoldout = false;

		[SerializeField]
		private int m_subShaderIdx = 0;

		[SerializeField]
		private string m_subShaderIdxStr = string.Empty;

		[SerializeField]
		private bool m_subStringFoldout = false;

		[SerializeField]
		private int m_subShaderLOD = -1;

		[SerializeField]
		private string m_subShaderLODStr;

		//[SerializeField]
		//private bool m_mainMPMasterNode = false;

		[NonSerialized]
		private TemplateMultiPass m_templateMultiPass;

		[SerializeField]
		private TemplateModulesHelper m_subShaderModule = new TemplateModulesHelper();

		[SerializeField]
		private TemplateModulesHelper m_passModule = new TemplateModulesHelper();

		[SerializeField]
		private string m_passName = string.Empty;

		[SerializeField]
		private string m_originalPassName = string.Empty;

		[SerializeField]
		private bool m_hasLinkPorts = false;

		[SerializeField]
		private bool m_isInvisible = false;

		[SerializeField]
		private bool m_invalidNode = false;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_masterNodeCategory = 1;// First Template
			m_marginPreviewLeft = 20;
			m_shaderNameIsTitle = false;
		}

		public override void ReleaseResources()
		{
			if( !m_isMainOutputNode )
				return;

			if( m_templateMultiPass != null && m_templateMultiPass.AvailableShaderProperties != null )
			{
				// Unregister old template properties
				int oldPropertyCount = m_templateMultiPass.AvailableShaderProperties.Count;
				for( int i = 0; i < oldPropertyCount; i++ )
				{
					UIUtils.ReleaseUniformName( UniqueId, m_templateMultiPass.AvailableShaderProperties[ i ].PropertyName );
				}
			}
		}

		void RegisterProperties()
		{
			if ( !m_isMainOutputNode )
			{
				m_reRegisterTemplateData = false;
				return;
			}

			if( m_templateMultiPass != null )
			{
				m_reRegisterTemplateData = false;
				// Register old template properties
				int newPropertyCount = m_templateMultiPass.AvailableShaderProperties.Count;
				for( int i = 0; i < newPropertyCount; i++ )
				{
					int nodeId = UIUtils.CheckUniformNameOwner( m_templateMultiPass.AvailableShaderProperties[ i ].PropertyName );
					if( nodeId > -1 )
					{
						ParentNode node = m_containerGraph.GetNode( nodeId );
						if( node != null )
						{
							UIUtils.ShowMessage( string.Format( "Template requires property name {0} which is currently being used by {1}. Please rename it and reload template.", m_templateMultiPass.AvailableShaderProperties[ i ].PropertyName, node.Attributes.Name ) );
						}
						else
						{
							UIUtils.ShowMessage( string.Format( "Template requires property name {0} which is currently being on your graph. Please rename it and reload template.", m_templateMultiPass.AvailableShaderProperties[ i ].PropertyName ) );
						}
					}
					else
					{
						UIUtils.RegisterUniformName( UniqueId, m_templateMultiPass.AvailableShaderProperties[ i ].PropertyName );
					}
				}
			}
		}

		public override void OnEnable()
		{
			base.OnEnable();
			m_reRegisterTemplateData = true;
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			if( UniqueId >= 0 )
			{
				m_containerGraph.MultiPassMasterNodes.AddNode( this );
			}
		}
		

		public void SetTemplate( TemplateMultiPass template, bool writeDefaultData, bool fetchMasterNodeCategory, int subShaderIdx, int passIdx )
		{
			if( subShaderIdx > -1 )
				m_subShaderIdx = subShaderIdx;

			if( passIdx > -1 )
				m_passIdx = passIdx;

			
			ReleaseResources();
			m_templateMultiPass = ( template == null ) ? TemplatesManager.GetTemplate( m_templateGUID ) as TemplateMultiPass : template;
			if( m_templateMultiPass != null )
			{
				if( m_templateMultiPass.IsSinglePass )
				{
					SetAdditonalTitleText( string.Empty );
				}
				else
				{
					SetAdditonalTitleText(string.Format( SubTitleFormatterStr, m_subShaderIdx, m_passIdx ));
				}
				m_invalidNode = false;
				if( m_subShaderIdx >= m_templateMultiPass.SubShaders.Count ||
					m_passIdx >= m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes.Count )
				{
					if( DebugConsoleWindow.DeveloperMode )
						Debug.LogFormat( "Inexisting pass {0}. Cancelling template fetch", m_originalPassName );

					return;
				}

				m_isMainOutputNode = m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].IsMainPass;
				if( m_isMainOutputNode )
				{
					// We cannot use UIUtils.MasterNodeOnTexture.height since this method can be 
					// called before UIUtils is initialized
					m_insideSize.y = 55;
				}
				else
				{
					m_insideSize.y = 0;
				}

				//IsMainOutputNode = m_mainMPMasterNode;
				m_isInvisible = m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].IsInvisible;

				m_originalPassName = m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].PassNameContainer.Data;

				m_shaderNameIsTitle = ( m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes.Count == 1 );

				if( m_templateMultiPass.SubShaders[ m_subShaderIdx ].LODContainer.Index > -1 )
				{
					m_subShaderLODStr = m_templateMultiPass.SubShaders[ m_subShaderIdx ].LODContainer.Id;
					m_subShaderLOD = Convert.ToInt32( m_templateMultiPass.SubShaders[ m_subShaderIdx ].LODContainer.Data );
				}
				else
				{
					m_subShaderLOD = -1;
				}
				m_fetchMasterNodeCategory = fetchMasterNodeCategory;
				m_templateGUID = m_templateMultiPass.GUID;
				UpdatePortInfo();
				//bool updateInfofromTemplate = UpdatePortInfo();
				//if( updateInfofromTemplate )
				//{
				m_subShaderModule.FetchDataFromTemplate( m_templateMultiPass.SubShaders[ m_subShaderIdx ].Modules );
				m_passModule.FetchDataFromTemplate( m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules );
				//}

				RegisterProperties();
				if( writeDefaultData )
				{
					ShaderName = m_templateMultiPass.DefaultShaderName;
					m_passName = m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].PassNameContainer.Data;
					if( !m_templateMultiPass.IsSinglePass && !m_shaderNameIsTitle )
					{
						SetClippedTitle( m_passName );
					}
				}

				UpdateSubShaderPassStr();

				if( m_isMainOutputNode )
					m_fireTemplateChange = true;
			}
			else
			{
				m_invalidNode = true;
			}
		}

		bool UpdatePortInfo()
		{
			List<TemplateInputData> inputDataList = m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].InputDataList;
			int count = inputDataList.Count;
			if( count != m_inputPorts.Count )
			{
				DeleteAllInputConnections( true );

				for( int i = 0; i < count; i++ )
				{
					InputPort port = AddInputPort( inputDataList[ i ].DataType, false, inputDataList[ i ].PortName, inputDataList[ i ].OrderId, inputDataList[ i ].PortCategory, inputDataList[ i ].PortUniqueId );
					port.ExternalLinkId = inputDataList[ i ].LinkId;
					m_hasLinkPorts = m_hasLinkPorts || !string.IsNullOrEmpty( inputDataList[ i ].LinkId );
				}
				return true;
			}
			else
			{
				for( int i = 0; i < count; i++ )
				{
					m_inputPorts[ i ].ChangeProperties( inputDataList[ i ].PortName, inputDataList[ i ].DataType, false );
					m_inputPorts[ i ].ExternalLinkId = inputDataList[ i ].LinkId;
				}
				return false;
			}
		}

		void SetCategoryIdxFromTemplate()
		{
			int templateCount = TemplatesManager.TemplateCount;
			for( int i = 0; i < templateCount; i++ )
			{
				int idx = i + 1;
				TemplateMultiPass templateData = TemplatesManager.GetTemplate( i ) as TemplateMultiPass;
				if( templateData != null && m_templateMultiPass != null && m_templateMultiPass.GUID.Equals( templateData.GUID ) )
					m_masterNodeCategory = idx;
			}
		}

		void CheckTemplateChanges()
		{
			if( m_invalidNode )
				return;

			if( m_isMainOutputNode )
			{
				if( m_containerGraph.MultiPassMasterNodes.Count != m_templateMultiPass.MasterNodesRequired )
				{
					if( m_availableCategories == null )
						RefreshAvailableCategories();

					if( DebugConsoleWindow.DeveloperMode )
						Debug.Log( "Template Pass amount was changed. Rebuiling master nodes" );

					m_containerGraph.ParentWindow.ReplaceMasterNode( m_availableCategories[ m_masterNodeCategory ], true );
				}
			}
		}
			
		public override void OnNodeLogicUpdate( DrawInfo drawInfo )
		{
			base.OnNodeLogicUpdate( drawInfo );
			if( m_invalidNode )
			{
				return;
			}

			if( m_templateMultiPass == null )
			{
				// Hotcode reload has happened
				SetTemplate( null, false, true, m_subShaderIdx, m_passIdx );
				CheckTemplateChanges();
			}

			if( m_reRegisterTemplateData )
			{
				RegisterProperties();
			}

			if( m_fetchMasterNodeCategory )
			{
				if( m_availableCategories != null )
				{
					m_fetchMasterNodeCategory = false;
					SetCategoryIdxFromTemplate();
				}
			}

			if( m_fireTemplateChange )
			{
				m_fireTemplateChange = false;
				m_containerGraph.FireMasterNodeReplacedEvent();
			}

		}

		public override void Draw( DrawInfo drawInfo )
		{
			if( !m_isInvisible )
			{
				base.Draw( drawInfo );
			}
		}
		
		public override void OnNodeLayout( DrawInfo drawInfo )
		{
			if( m_invalidNode )
			{
				if( m_isMainOutputNode )
				{
					UIUtils.ShowMessage( "Invalid current template. Switching to Standard Surface", MessageSeverity.Error );
					m_shaderModelIdx = 0;
					m_masterNodeCategory = 0;
					m_containerGraph.ParentWindow.ReplaceMasterNode( new MasterNodeCategoriesData( AvailableShaderTypes.SurfaceShader, m_shaderName ), false );
				}
				return;
			}

			if( m_isInvisible )
			{
				return;
			}

			if( !IsMainOutputNode )
			{
				if( Docking )
				{
					m_useSquareNodeTitle = true;
					TemplateMultiPassMasterNode master = ContainerGraph.CurrentMasterNode as TemplateMultiPassMasterNode;
					m_position = master.TruePosition;
					m_position.height = 32;
					int masterIndex = ContainerGraph.MultiPassMasterNodes.NodesList.IndexOf( master );
					int index = ContainerGraph.MultiPassMasterNodes.GetNodeRegisterIdx( UniqueId );
					if( index > masterIndex )
					{
						int backTracking = 0;
						for( int i = index - 1; i > masterIndex; i-- )
						{
							if( ContainerGraph.MultiPassMasterNodes.NodesList[ i ].Docking )
								backTracking++;
						}
						m_position.y = master.TruePosition.yMax + 1 + 33 * ( backTracking );// ContainerGraph.MultiPassMasterNodes.NodesList[ index - 1 ].TruePosition.yMax;
						base.OnNodeLayout( drawInfo );
					}
					else
					{
						int forwardTracking = 1;
						for( int i = index + 1; i < masterIndex; i++ )
						{
							if( ContainerGraph.MultiPassMasterNodes.NodesList[ i ].Docking )
								forwardTracking++;
						}
						m_position.y = master.TruePosition.y - 33 * ( forwardTracking );// ContainerGraph.MultiPassMasterNodes.NodesList[ index - 1 ].TruePosition.yMax;
						base.OnNodeLayout( drawInfo );
					}
				}
				else
				{
					m_useSquareNodeTitle = false;
					base.OnNodeLayout( drawInfo );
				}
			} else
			{
				base.OnNodeLayout( drawInfo );
			}
		}

		public override void OnNodeRepaint( DrawInfo drawInfo )
		{
			base.OnNodeRepaint( drawInfo );
			if( m_invalidNode )
				return;

			if( !m_isInvisible )
			{
				if( m_containerGraph.IsInstancedShader )
				{
					DrawInstancedIcon( drawInfo );
				}
			}
		}

		public override void UpdateFromShader( Shader newShader )
		{
			if( m_currentMaterial != null )
			{
				m_currentMaterial.shader = newShader;
			}
			CurrentShader = newShader;
		}

		public override void UpdateMasterNodeMaterial( Material material )
		{
			m_currentMaterial = material;
			FireMaterialChangedEvt();
		}

		void DrawOpenTemplateButton()
		{
			if( GUILayout.Button( OpenTemplateStr ) && m_templateMultiPass != null )
			{
				try
				{
					string pathname = AssetDatabase.GUIDToAssetPath( m_templateMultiPass.GUID );
					if( !string.IsNullOrEmpty( pathname ) )
					{
						Shader selectedTemplate = AssetDatabase.LoadAssetAtPath<Shader>( pathname );
						if( selectedTemplate != null )
						{
							AssetDatabase.OpenAsset( selectedTemplate, 1 );
						}
					}
				}
				catch( Exception e )
				{
					Debug.LogException( e );
				}
			}
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			if( m_invalidNode )
				return;

			NodeUtils.DrawPropertyGroup( ref m_propertiesFoldout, CommonPropertiesStr, DrawCommonProperties );
			NodeUtils.DrawPropertyGroup( ref m_subStringFoldout, SubShaderModuleStr, DrawSubShaderProperties );
			NodeUtils.DrawPropertyGroup( ref m_passFoldout, PassModuleStr, DrawPassProperties );

			DrawMaterialInputs( UIUtils.MenuItemToolbarStyle, false );

			if( m_propertyOrderChanged )
			{
				List<TemplateMultiPassMasterNode> mpNodes = UIUtils.CurrentWindow.CurrentGraph.MultiPassMasterNodes.NodesList;
				int count = mpNodes.Count;
				for( int i = 0; i < count; i++ )
				{
					if( mpNodes[ i ].UniqueId != UniqueId )
					{
						mpNodes[ i ].CopyPropertyListFrom( this );
					}
				}
			}

#if SHOW_TEMPLATE_HELP_BOX
			EditorGUILayout.HelpBox( WarningMessage, MessageType.Warning );
#endif
		}

		void DrawCommonProperties()
		{
			if( m_isMainOutputNode )
			{
				DrawShaderName();
				DrawCurrentShaderType();
				EditorGUI.BeginChangeCheck();
				DrawPrecisionProperty();
				if( EditorGUI.EndChangeCheck() )
					ContainerGraph.CurrentPrecision = m_currentPrecisionType;

				DrawCustomInspector();
			}
			EditorGUILayout.LabelField( m_subShaderIdxStr );
			EditorGUILayout.LabelField( m_passIdxStr );

			DrawOpenTemplateButton();
		}

		void DrawSubShaderProperties()
		{
			bool noValidData = true;
			if( m_subShaderLOD > -1 )
			{
				noValidData = false;
				EditorGUILayout.LabelField( m_subShaderLODStr );
			}

			if( m_subShaderModule.HasValidData )
			{
				noValidData = false;
				m_subShaderModule.Draw( this, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Modules );
				if( m_subShaderModule.IsDirty )
				{
					List<TemplateMultiPassMasterNode> mpNodes = UIUtils.CurrentWindow.CurrentGraph.MultiPassMasterNodes.NodesList;
					int count = mpNodes.Count;
					for( int i = 0; i < count; i++ )
					{
						if( mpNodes[ i ].SubShaderIdx == m_subShaderIdx && mpNodes[ i ].UniqueId != UniqueId )
						{
							mpNodes[ i ].SubShaderModule.CopyFrom( m_subShaderModule );
						}
					}
					m_subShaderModule.IsDirty = false;
				}
			}
			
			if( noValidData )
			{
				EditorGUILayout.HelpBox( NoSubShaderPropertyStr, MessageType.Info );
			}
		}

		void DrawPassProperties()
		{
			EditorGUI.BeginChangeCheck();
			m_passName = EditorGUILayoutTextField( PassNameStr, m_passName );
			if( EditorGUI.EndChangeCheck() )
			{
				if( m_passName.Length > 0 )
				{
					m_passName = UIUtils.RemoveShaderInvalidCharacters( m_passName );
				}
				else
				{
					m_passName = m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].PassNameContainer.Data;
				}
				if( !m_templateMultiPass.IsSinglePass )
					SetClippedTitle( m_passName );
			}

			if( m_passModule.HasValidData )
			{
				m_passModule.Draw( this, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules );
			}
		}

		bool CreateInstructionsForList( TemplateData templateData, ref List<InputPort> ports, ref string shaderBody, ref List<string> vertexInstructions, ref List<string> fragmentInstructions )
		{
			if( ports.Count == 0 )
				return true;

			bool isValid = true;
			//UIUtils.CurrentWindow.CurrentGraph.ResetNodesLocalVariables();
			for( int i = 0; i < ports.Count; i++ )
			{
				TemplateInputData inputData = templateData.InputDataFromId( ports[ i ].PortId );
				if( ports[ i ].IsConnected || ports[ i ].HasConnectedExternalLink )
				{
					if( m_templateMultiPass.SubShaders[ m_subShaderIdx ].Modules.SRPType != TemplateSRPType.BuiltIn )
					{
						if( ports[ i ].Name.Contains( "Normal" ) )
						{
							m_currentDataCollector.AddToDefines( UniqueId, "_NORMALMAP 1" );
						}

						if( ports[ i ].Name.Contains( "Alpha Clip Threshold" ) )
						{
							m_currentDataCollector.AddToDefines( UniqueId, "_AlphaClip 1" );
						}
					}

					m_currentDataCollector.ResetInstructions();
					m_currentDataCollector.ResetVertexInstructions();

					m_currentDataCollector.PortCategory = ports[ i ].Category;
					string newPortInstruction = ports[ i ].GeneratePortInstructions( ref m_currentDataCollector );

					if( m_currentDataCollector.DirtySpecialLocalVariables )
					{
						string cleanVariables = m_currentDataCollector.SpecialLocalVariables.Replace( "\t", string.Empty );
						m_currentDataCollector.AddInstructions( cleanVariables, false );
						m_currentDataCollector.ClearSpecialLocalVariables();
					}

					if( m_currentDataCollector.DirtyVertexVariables )
					{
						string cleanVariables = m_currentDataCollector.VertexLocalVariables.Replace( "\t", string.Empty );
						m_currentDataCollector.AddVertexInstruction( cleanVariables, UniqueId, false );
						m_currentDataCollector.ClearVertexLocalVariables();
					}

					// fill functions 
					for( int j = 0; j < m_currentDataCollector.InstructionsList.Count; j++ )
					{
						fragmentInstructions.Add( m_currentDataCollector.InstructionsList[ j ].PropertyName );
					}

					for( int j = 0; j < m_currentDataCollector.VertexDataList.Count; j++ )
					{
						vertexInstructions.Add( m_currentDataCollector.VertexDataList[ j ].PropertyName );
					}

					m_templateMultiPass.SetPassInputData( m_subShaderIdx, m_passIdx, ports[ i ].PortId, newPortInstruction );
					isValid = m_templateMultiPass.FillTemplateBody( m_subShaderIdx, m_passIdx, inputData.TagId, ref shaderBody, newPortInstruction ) && isValid;
				}
				else
				{
					m_templateMultiPass.SetPassInputData( m_subShaderIdx, m_passIdx, ports[ i ].PortId, inputData.DefaultValue );
					isValid = m_templateMultiPass.FillTemplateBody( m_subShaderIdx, m_passIdx, inputData.TagId, ref shaderBody, inputData.DefaultValue ) && isValid;
				}
			}
			return isValid;
		}
		
		public string BuildShaderBody()
		{
			List<TemplateMultiPassMasterNode> list = UIUtils.CurrentWindow.CurrentGraph.MultiPassMasterNodes.NodesList;
			int currentSubshader = list[ 0 ].SubShaderIdx;
			m_templateMultiPass.SetShaderName( string.Format( TemplatesManager.NameFormatter, m_shaderName ) );
			if( string.IsNullOrEmpty( m_customInspectorName ) )
			{
				m_templateMultiPass.SetCustomInspector( string.Empty );
			}
			else
			{
				m_templateMultiPass.SetCustomInspector( CustomInspectorFormatted );
			}

			MasterNodeDataCollector dataCollector = new MasterNodeDataCollector();
			int count = list.Count;
			for( int i = 0; i < count; i++ )
			{
				list[ i ].CollectData();
				list[ i ].FillPassData();

				if( list[ i ].SubShaderIdx == currentSubshader )
				{
					dataCollector.CopyPropertiesFromDataCollector( list[ i ].CurrentDataCollector );
				}
				else
				{
					list[ i - 1 ].FillSubShaderData( dataCollector );
					dataCollector.Destroy();
					dataCollector = new MasterNodeDataCollector();
					dataCollector.CopyPropertiesFromDataCollector( list[ i ].CurrentDataCollector );

					currentSubshader = list[ i ].SubShaderIdx;
				}

				if( i == ( count - 1 ) )
				{
					list[ i ].FillSubShaderData( dataCollector );
				}
			}
			return list[ 0 ].CurrentTemplate.IdManager.BuildShader();
		}


		public override Shader Execute( string pathname, bool isFullPath )
		{
			m_templateMultiPass.Reset();
			ForceReordering();
			base.Execute( pathname, isFullPath );
			string shaderBody = BuildShaderBody();
			UpdateShaderAsset( ref pathname, ref shaderBody, isFullPath );
			return m_currentShader;
		}

		public void CollectData()
		{
			if( m_inputPorts.Count == 0 )
				return;

			ContainerGraph.ResetNodesLocalVariables();
			m_currentDataCollector = new MasterNodeDataCollector( this );
			
			m_currentDataCollector.TemplateDataCollectorInstance.SetMultipassInfo( m_subShaderIdx, m_passIdx, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Modules.SRPType );
			m_currentDataCollector.TemplateDataCollectorInstance.FillSpecialVariables( m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ] );
			SetupNodeCategories();

			TemplateData templateData = m_templateMultiPass.CreateTemplateData( m_shaderName, string.Empty, m_subShaderIdx, m_passIdx );
			m_currentDataCollector.TemplateDataCollectorInstance.BuildFromTemplateData( m_currentDataCollector, templateData );

			if( m_currentDataCollector.TemplateDataCollectorInstance.InterpData.DynamicMax )
			{
				int interpolatorAmount = -1;
				if( m_passModule.ShaderModelHelper.ValidData )
				{
					interpolatorAmount = m_passModule.ShaderModelHelper.InterpolatorAmount;
				}
				else if( m_subShaderModule.ShaderModelHelper.ValidData )
				{
					interpolatorAmount = m_subShaderModule.ShaderModelHelper.InterpolatorAmount;
				}

				if( interpolatorAmount > -1 )
				{
					m_currentDataCollector.TemplateDataCollectorInstance.InterpData.RecalculateAvailableInterpolators( interpolatorAmount );
				}
			}

			//Copy Properties
			{
				int shaderPropertiesAmount = m_templateMultiPass.AvailableShaderProperties.Count;
				for( int i = 0; i < shaderPropertiesAmount; i++ )
				{
					m_currentDataCollector.SoftRegisterUniform( m_templateMultiPass.AvailableShaderProperties[ i ].PropertyName );
				}
			}
			//Copy Globals from SubShader level
			{
				int subShaderGlobalAmount = m_templateMultiPass.SubShaders[ m_subShaderIdx ].AvailableShaderGlobals.Count;
				for( int i = 0; i < subShaderGlobalAmount; i++ )
				{
					m_currentDataCollector.SoftRegisterUniform( m_templateMultiPass.SubShaders[ m_subShaderIdx ].AvailableShaderGlobals[ i ].PropertyName );
				}
			}
			//Copy Globals from Pass Level
			{
				int passGlobalAmount = m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].AvailableShaderGlobals.Count;
				for( int i = 0; i < passGlobalAmount; i++ )
				{
					m_currentDataCollector.SoftRegisterUniform( m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].AvailableShaderGlobals[ i ].PropertyName );
				}
			}

			RegisterStandaloneFuntions();
			m_containerGraph.CheckPropertiesAutoRegister( ref m_currentDataCollector );

			//Sort ports by both 
			List<InputPort> fragmentPorts = new List<InputPort>();
			List<InputPort> vertexPorts = new List<InputPort>();

			SortInputPorts( ref vertexPorts, ref fragmentPorts );


			string shaderBody = templateData.TemplateBody;

			List<string> vertexInstructions = new List<string>();
			List<string> fragmentInstructions = new List<string>();

			bool validBody = true;

			//validBody = CreateInstructionsForList( templateData, ref fragmentPorts, ref shaderBody, ref vertexInstructions, ref fragmentInstructions ) && validBody;
			//ContainerGraph.ResetNodesLocalVariablesIfNot( MasterNodePortCategory.Vertex );
			//validBody = CreateInstructionsForList( templateData, ref vertexPorts, ref shaderBody, ref vertexInstructions, ref fragmentInstructions ) && validBody;
			validBody = CreateInstructionsForList( templateData, ref vertexPorts, ref shaderBody, ref vertexInstructions, ref fragmentInstructions ) && validBody;
			validBody = CreateInstructionsForList( templateData, ref fragmentPorts, ref shaderBody, ref vertexInstructions, ref fragmentInstructions ) && validBody;

			templateData.ResetTemplateUsageData();

			// Fill vertex interpolators assignment
			for( int i = 0; i < m_currentDataCollector.VertexInterpDeclList.Count; i++ )
			{
				vertexInstructions.Add( m_currentDataCollector.VertexInterpDeclList[ i ] );
			}

			vertexInstructions.AddRange( m_currentDataCollector.TemplateDataCollectorInstance.GetInterpUnusedChannels() );

			//Fill common local variables and operations
			validBody = m_templateMultiPass.FillVertexInstructions( m_subShaderIdx, m_passIdx, vertexInstructions.ToArray() ) && validBody;
			validBody = m_templateMultiPass.FillFragmentInstructions( m_subShaderIdx, m_passIdx, fragmentInstructions.ToArray() ) && validBody;

			vertexInstructions.Clear();
			vertexInstructions = null;

			fragmentInstructions.Clear();
			fragmentInstructions = null;

			// Add Instanced Properties
			if( m_containerGraph.IsInstancedShader )
			{
				m_currentDataCollector.TabifyInstancedVars();
				m_currentDataCollector.InstancedPropertiesList.Insert( 0, new PropertyDataCollector( -1, string.Format( IOUtils.InstancedPropertiesBegin, UIUtils.RemoveInvalidCharacters( m_shaderName ) ) ) );
				m_currentDataCollector.InstancedPropertiesList.Add( new PropertyDataCollector( -1, IOUtils.InstancedPropertiesEnd ) );
				m_currentDataCollector.UniformsList.AddRange( m_currentDataCollector.InstancedPropertiesList );
			}

			//Add Functions
			m_currentDataCollector.UniformsList.AddRange( m_currentDataCollector.FunctionsList );
		}

		public void FillSubShaderData( MasterNodeDataCollector dataCollector = null )
		{
			MasterNodeDataCollector currDataCollector = ( dataCollector == null ) ? m_currentDataCollector : dataCollector;
			// SubShader Data
			
			m_templateMultiPass.SetPropertyData( currDataCollector.BuildUnformatedPropertiesStringArr() );
			m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModulePass, m_subShaderIdx, currDataCollector.GrabPassList );
			SetModuleData( m_subShaderModule, true );
		}

		public void FillPassData()
		{
			if( m_isInvisible )
			{
				int inputCount = m_inputPorts.Count;
				for( int i = 0; i < inputCount; i++ )
				{
					if( m_inputPorts[ i ].HasExternalLink )
					{
						TemplateMultiPassMasterNode linkedNode = m_inputPorts[ i ].ExternalLinkNode as TemplateMultiPassMasterNode;
						if( linkedNode != null )
						{
							SetLinkedModuleData( linkedNode.PassModule );
						}
					}
				}
			}

			SetModuleData( m_passModule, false );
			if( m_currentDataCollector != null )
			{
				m_templateMultiPass.SetPassData( TemplateModuleDataType.PassVertexData, m_subShaderIdx, m_passIdx, m_currentDataCollector.VertexInputList.ToArray() );
				m_templateMultiPass.SetPassData( TemplateModuleDataType.PassInterpolatorData, m_subShaderIdx, m_passIdx, m_currentDataCollector.InterpolatorList.ToArray() );

				List<PropertyDataCollector> includePragmaDefineList = new List<PropertyDataCollector>();
				includePragmaDefineList.AddRange( m_currentDataCollector.IncludesList );
				includePragmaDefineList.AddRange( m_currentDataCollector.DefinesList );
				includePragmaDefineList.AddRange( m_currentDataCollector.PragmasList );

				m_templateMultiPass.SetPassData( TemplateModuleDataType.ModulePragma, m_subShaderIdx, m_passIdx, includePragmaDefineList );
				m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleGlobals, m_subShaderIdx, m_passIdx, m_currentDataCollector.UniformsList );
				m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleInputVert, m_subShaderIdx, m_passIdx, m_currentDataCollector.TemplateDataCollectorInstance.VertexInputParamsStr );
				m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleInputFrag, m_subShaderIdx, m_passIdx, m_currentDataCollector.TemplateDataCollectorInstance.FragInputParamsStr );
			}
			m_templateMultiPass.SetPassData( TemplateModuleDataType.PassNameData, m_subShaderIdx, m_passIdx, string.Format( PassNameFormateStr, m_passName ) );
		}

		void SetLinkedModuleData( TemplateModulesHelper linkedModule )
		{
				if(	linkedModule.AdditionalPragmas.ValidData )
				{
					linkedModule.AdditionalPragmas.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules.IncludePragmaContainer );
				}

				if( linkedModule.AdditionalIncludes.ValidData )
				{
					linkedModule.AdditionalIncludes.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules.IncludePragmaContainer );
				}

				if( linkedModule.AdditionalDefines.ValidData )
				{
					linkedModule.AdditionalDefines.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules.IncludePragmaContainer );
				}
		}

		void SetModuleData( TemplateModulesHelper module, bool isSubShader )
		{
			if( isSubShader )
			{
				if ( module.AdditionalPragmas.ValidData )
				{
					module.AdditionalPragmas.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Modules.IncludePragmaContainer );
				}

				if ( module.AdditionalIncludes.ValidData )
				{
					module.AdditionalIncludes.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Modules.IncludePragmaContainer );
				}

				if ( module.AdditionalDefines.ValidData )
				{
					module.AdditionalDefines.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Modules.IncludePragmaContainer );
				}

				if ( module.ShaderModelHelper.ValidData )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleShaderModel, m_subShaderIdx, module.ShaderModelHelper.GenerateShaderData() );
				}

				if( module.TagsHelper.ValidData )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleTag, m_subShaderIdx, module.TagsHelper.GenerateTags() );
				}

				if( module.BlendOpHelper.ValidBlendMode )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleBlendMode, m_subShaderIdx, module.BlendOpHelper.CurrentBlendFactor );
				}

				if( module.BlendOpHelper.ValidBlendOp )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleBlendOp, m_subShaderIdx, module.BlendOpHelper.CurrentBlendOp );
				}

				if( module.CullModeHelper.ValidData )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleCullMode, m_subShaderIdx, module.CullModeHelper.GenerateShaderData() );
				}

				if( module.ColorMaskHelper.ValidData )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleColorMask, m_subShaderIdx, module.ColorMaskHelper.GenerateShaderData() );
				}

				if( module.DepthOphelper.ValidZTest )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleZTest, m_subShaderIdx, module.DepthOphelper.CurrentZTestMode );
				}

				if( module.DepthOphelper.ValidZWrite )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleZwrite, m_subShaderIdx, module.DepthOphelper.CurrentZWriteMode );
				}

				if( module.DepthOphelper.ValidOffset )
				{
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleZOffset, m_subShaderIdx, module.DepthOphelper.CurrentOffset );
				}

				if( module.StencilBufferHelper.ValidData )
				{
					CullMode cullMode = ( module.CullModeHelper.ValidData ) ? module.CullModeHelper.CurrentCullMode : CullMode.Back;
					string value = module.StencilBufferHelper.CreateStencilOp( cullMode );
					m_templateMultiPass.SetSubShaderData( TemplateModuleDataType.ModuleStencil, m_subShaderIdx, value.Split( '\n' ) );
				}
			}
			else
			{
				if ( module.AdditionalPragmas.ValidData )
				{
					module.AdditionalPragmas.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules.IncludePragmaContainer );
				}

				if ( module.AdditionalIncludes.ValidData )
				{
					module.AdditionalIncludes.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules.IncludePragmaContainer );
				}

				if ( module.AdditionalDefines.ValidData )
				{
					module.AdditionalDefines.AddToDataCollector( ref m_currentDataCollector, m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].Modules.IncludePragmaContainer );
				}

				if ( module.ShaderModelHelper.ValidData )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleShaderModel, m_subShaderIdx, m_passIdx, module.ShaderModelHelper.GenerateShaderData() );
				}

				if( module.TagsHelper.ValidData )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleTag, m_subShaderIdx, m_passIdx, module.TagsHelper.GenerateTags() );
				}

				if( module.BlendOpHelper.ValidBlendMode )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleBlendMode, m_subShaderIdx, m_passIdx, module.BlendOpHelper.CurrentBlendFactor );
				}

				if( module.BlendOpHelper.ValidBlendOp )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleBlendOp, m_subShaderIdx, m_passIdx, module.BlendOpHelper.CurrentBlendOp );
				}

				if( module.CullModeHelper.ValidData )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleCullMode, m_subShaderIdx, m_passIdx, module.CullModeHelper.GenerateShaderData() );
				}

				if( module.ColorMaskHelper.ValidData )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleColorMask, m_subShaderIdx, m_passIdx, module.ColorMaskHelper.GenerateShaderData() );
				}

				if( module.DepthOphelper.ValidZTest )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleZTest, m_subShaderIdx, m_passIdx, module.DepthOphelper.CurrentZTestMode );
				}

				if( module.DepthOphelper.ValidZWrite )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleZwrite, m_subShaderIdx, m_passIdx, module.DepthOphelper.CurrentZWriteMode );
				}

				if( module.DepthOphelper.ValidOffset )
				{
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleZOffset, m_subShaderIdx, m_passIdx, module.DepthOphelper.CurrentOffset );
				}

				if( module.StencilBufferHelper.ValidData )
				{
					CullMode cullMode = ( module.CullModeHelper.ValidData ) ? module.CullModeHelper.CurrentCullMode : CullMode.Back;
					string value = module.StencilBufferHelper.CreateStencilOp( cullMode );
					m_templateMultiPass.SetPassData( TemplateModuleDataType.ModuleStencil, m_subShaderIdx, m_passIdx, value.Split( '\n' ) );
				}
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			return "0";
		}

		public override void Destroy()
		{
			base.Destroy();
			m_subShaderModule.Destroy();
			m_subShaderModule = null;
			m_passModule.Destroy();
			m_passModule = null;
			m_containerGraph.MultiPassMasterNodes.RemoveNode( this );
		}

		void UpdateSubShaderPassStr()
		{
			m_subShaderIdxStr = SubShaderModuleStr + m_templateMultiPass.SubShaders[ m_subShaderIdx].Idx;
			m_passIdxStr = PassModuleStr + m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[m_passIdx].Idx;
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			try
			{
				string currShaderName = GetCurrentParam( ref nodeParams );
				if( currShaderName.Length > 0 )
					currShaderName = UIUtils.RemoveShaderInvalidCharacters( currShaderName );

				m_templateGUID = GetCurrentParam( ref nodeParams );

				m_subShaderIdx = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
				m_passIdx = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
				m_passName = GetCurrentParam( ref nodeParams );
				SetTemplate( null, false, true, m_subShaderIdx, m_passIdx );
				// only in here, after SetTemplate, we know if shader name is to be used as title or not
				ShaderName = currShaderName;
				m_visiblePorts = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
				m_subShaderModule.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
				m_passModule.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
				if( m_templateMultiPass!= null && !m_templateMultiPass.IsSinglePass )
				{
					SetClippedTitle( m_passName );
				}
			}
			catch( Exception e )
			{
				Debug.LogException( e, this );
			}

			m_containerGraph.CurrentCanvasMode = NodeAvailability.TemplateShader;
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, ShaderName );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_templateGUID );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_subShaderIdx );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_passIdx );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_passName );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_visiblePorts );
			m_subShaderModule.WriteToString( ref nodeInfo );
			m_passModule.WriteToString( ref nodeInfo );
		}

		public override void ReadFromDeprecated( ref string[] nodeParams, Type oldType = null )
		{
			base.ReadFromString( ref nodeParams );
			try
			{
				string currShaderName = GetCurrentParam( ref nodeParams );
				if( currShaderName.Length > 0 )
					currShaderName = UIUtils.RemoveShaderInvalidCharacters( currShaderName );

				string templateGUID = GetCurrentParam( ref nodeParams );
				string templateShaderName = string.Empty;
				if( UIUtils.CurrentShaderVersion() > 13601 )
				{
					templateShaderName = GetCurrentParam( ref nodeParams );
				}

				TemplateMultiPass template = TemplatesManager.GetTemplate( templateGUID ) as TemplateMultiPass;
				if( template != null )
				{
					m_templateGUID = templateGUID;
					SetTemplate( null, false, true, 0, 0 );
				}
				else
				{
					template = TemplatesManager.GetTemplateByName( templateShaderName ) as TemplateMultiPass;
					if( template != null )
					{
						m_templateGUID = template.GUID;
						SetTemplate( null, false, true, 0, 0 );
					}
					else
					{
						m_masterNodeCategory = -1;
					}
				}

				if( m_invalidNode )
					return;

				// only in here, after SetTemplate, we know if shader name is to be used as title or not
				ShaderName = currShaderName;
				if( UIUtils.CurrentShaderVersion() > 13902 )
				{

					//BLEND MODULE
					if( m_templateMultiPass.SubShaders[ 0 ].Modules.BlendData.ValidBlendMode )
					{
						m_subShaderModule.BlendOpHelper.ReadBlendModeFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.BlendData.ValidBlendMode )
					{
						m_passModule.BlendOpHelper.ReadBlendModeFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

					if( m_templateMultiPass.SubShaders[ 0 ].Modules.BlendData.ValidBlendOp )
					{
						m_subShaderModule.BlendOpHelper.ReadBlendOpFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.BlendData.ValidBlendOp )
					{
						m_passModule.BlendOpHelper.ReadBlendOpFromString( ref m_currentReadParamIdx, ref nodeParams );
					}


					//CULL MODE
					if( m_templateMultiPass.SubShaders[ 0 ].Modules.CullModeData.DataCheck == TemplateDataCheck.Valid )
					{
						m_subShaderModule.CullModeHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.CullModeData.DataCheck == TemplateDataCheck.Valid )
					{
						m_passModule.CullModeHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

					//COLOR MASK
					if( m_templateMultiPass.SubShaders[ 0 ].Modules.ColorMaskData.DataCheck == TemplateDataCheck.Valid )
					{
						m_subShaderModule.ColorMaskHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.ColorMaskData.DataCheck == TemplateDataCheck.Valid )
					{
						m_passModule.ColorMaskHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

					//STENCIL BUFFER
					if( m_templateMultiPass.SubShaders[ 0 ].Modules.StencilData.DataCheck == TemplateDataCheck.Valid )
					{
						m_subShaderModule.StencilBufferHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.StencilData.DataCheck == TemplateDataCheck.Valid )
					{
						m_passModule.StencilBufferHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

				}

				if( UIUtils.CurrentShaderVersion() > 14202 )
				{
					//DEPTH OPTIONS
					if( m_templateMultiPass.SubShaders[ 0 ].Modules.DepthData.ValidZWrite )
					{
						m_subShaderModule.DepthOphelper.ReadZWriteFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.DepthData.ValidZWrite )
					{
						m_passModule.DepthOphelper.ReadZWriteFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

					if( m_templateMultiPass.SubShaders[ 0 ].Modules.DepthData.ValidZTest )
					{
						m_subShaderModule.DepthOphelper.ReadZTestFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.DepthData.ValidZTest )
					{
						m_subShaderModule.DepthOphelper.ReadZTestFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

					if( m_templateMultiPass.SubShaders[ 0 ].Modules.DepthData.ValidOffset )
					{
						m_subShaderModule.DepthOphelper.ReadOffsetFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.DepthData.ValidOffset )
					{
						m_passModule.DepthOphelper.ReadOffsetFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

				}

				//TAGS
				if( UIUtils.CurrentShaderVersion() > 14301 )
				{
					if( m_templateMultiPass.SubShaders[ 0 ].Modules.TagData.DataCheck == TemplateDataCheck.Valid )
					{
						m_subShaderModule.TagsHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}
					else if( m_templateMultiPass.SubShaders[ 0 ].Passes[ 0 ].Modules.TagData.DataCheck == TemplateDataCheck.Valid )
					{
						m_passModule.TagsHelper.ReadFromString( ref m_currentReadParamIdx, ref nodeParams );
					}

				}
			}
			catch( Exception e )
			{
				Debug.LogException( e, this );
			}
			m_containerGraph.CurrentCanvasMode = NodeAvailability.TemplateShader;
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			CheckTemplateChanges();
		}

		public override float HeightEstimate
		{
			get
			{
				float heightEstimate = 0;
				heightEstimate = 32 + Constants.INPUT_PORT_DELTA_Y;
				if( m_templateMultiPass != null && !m_templateMultiPass.IsSinglePass )
				{
					heightEstimate += 22;
				}
				float internalPortSize = 0;
				for( int i = 0; i < InputPorts.Count; i++ )
				{
					if( InputPorts[ i ].Visible )
						internalPortSize += 18 + Constants.INPUT_PORT_DELTA_Y;
				}

				return heightEstimate + Mathf.Max( internalPortSize, m_insideSize.y );
			}
		}


		public int SubShaderIdx { get { return m_subShaderIdx; } }
		public int PassIdx { get { return m_passIdx; } }
		public TemplateMultiPass CurrentTemplate { get { return m_templateMultiPass; } }
		public TemplateModulesHelper SubShaderModule { get { return m_subShaderModule; } }
		public TemplateModulesHelper PassModule { get { return m_passModule; } }
		public string PassName { get { return m_templateMultiPass.SubShaders[ m_subShaderIdx ].Passes[ m_passIdx ].PassNameContainer.Data; } }
		public string OriginalPassName { get { return m_originalPassName; } }
		public bool HasLinkPorts { get { return m_hasLinkPorts; } }
	}
}
