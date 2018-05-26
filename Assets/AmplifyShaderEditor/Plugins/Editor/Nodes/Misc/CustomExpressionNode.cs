// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{

	public enum CustomExpressionMode
	{
		Create,
		Call
	}

	[Serializable]
	[NodeAttributes( "Custom Expression", "Miscellaneous", "Creates a custom expression or function if <b>return</b> is detected in the written code." )]
	public sealed class CustomExpressionNode : ParentNode
	{
		private const string CustomExpressionInfo = "Creates a custom expression or function according to how code is written on text area.\n\n" +
													" - If a return function is detected on Code text area then a function will be created.\n" +
													"Also in function mode a ; is expected on the end of each instruction line.\n\n" +
													"- If no return function is detected then an expression will be generated and used directly on the vertex/frag body.\n" +
													"On Expression mode a ; is not required on the end of an instruction line.";
		private const char LineFeedSeparator = '$';
		private const char SemiColonSeparator = '@';
		private const string ReturnHelper = "return";
		private const double MaxTimestamp = 1;
		private const string DefaultExpressionName = "My Custom Expression";
		private const string DefaultInputName = "In";
		private const string CodeTitleStr = "Code";
		private const string OutputTypeStr = "Output Type";
		private const string InputsStr = "Inputs";
		private const string InputNameStr = "Name";
		private const string InputTypeStr = "Type";
		private const string InputValueStr = "Value";
		private const string InputQualifierStr = "Qualifier";
		private const string ExpressionNameLabel = "Name";
		private const string FunctionCallMode = "Mode";
		private const string GenerateUniqueName = "Set Unique";
		private const string AutoRegister = "Auto-Register";

		private readonly string[] AvailableWireTypesStr =
		{
		"int",
		"float",
		"float2",
		"float3",
		"float4",
		"float3x3",
		"float4x4",
		"sampler1D",
		"sampler2D",
		"sampler3D",
		"samplerCUBE"};

		private readonly string[] AvailableOutputWireTypesStr =
		{
		"int",
		"float",
		"float2",
		"float3",
		"float4",
		"float3x3",
		"float4x4",
		"void",
		};

		private readonly string[] QualifiersStr =
		{
			"In",
			"Out",
			"InOut"
		};

		private readonly WirePortDataType[] AvailableWireTypes =
		{
			WirePortDataType.INT,
			WirePortDataType.FLOAT,
			WirePortDataType.FLOAT2,
			WirePortDataType.FLOAT3,
			WirePortDataType.FLOAT4,
			WirePortDataType.FLOAT3x3,
			WirePortDataType.FLOAT4x4,
			WirePortDataType.SAMPLER1D,
			WirePortDataType.SAMPLER2D,
			WirePortDataType.SAMPLER3D,
			WirePortDataType.SAMPLERCUBE
		};

		private readonly WirePortDataType[] AvailableOutputWireTypes =
		{
			WirePortDataType.INT,
			WirePortDataType.FLOAT,
			WirePortDataType.FLOAT2,
			WirePortDataType.FLOAT3,
			WirePortDataType.FLOAT4,
			WirePortDataType.FLOAT3x3,
			WirePortDataType.FLOAT4x4,
			WirePortDataType.OBJECT,
		};


		private readonly Dictionary<WirePortDataType, int> WireToIdx = new Dictionary<WirePortDataType, int>
		{
			{ WirePortDataType.INT,         0},
			{ WirePortDataType.FLOAT,       1},
			{ WirePortDataType.FLOAT2,      2},
			{ WirePortDataType.FLOAT3,      3},
			{ WirePortDataType.FLOAT4,      4},
			{ WirePortDataType.FLOAT3x3,    5},
			{ WirePortDataType.FLOAT4x4,    6},
			{ WirePortDataType.SAMPLER1D,   7},
			{ WirePortDataType.SAMPLER2D,   8},
			{ WirePortDataType.SAMPLER3D,   9},
			{ WirePortDataType.SAMPLERCUBE, 10}
		};

		[SerializeField]
		private string m_customExpressionName = DefaultExpressionName;

		[SerializeField]
		private List<bool> m_foldoutValuesFlags = new List<bool>();

		[SerializeField]
		private List<string> m_foldoutValuesLabels = new List<string>();

		[SerializeField]
		private List<VariableQualifiers> m_variableQualifiers = new List<VariableQualifiers>();

		[SerializeField]
		private string m_code = " ";

		[SerializeField]
		private int m_outputTypeIdx = 1;

		[SerializeField]
		private bool m_visibleInputsFoldout = true;

		[SerializeField]
		private CustomExpressionMode m_mode = CustomExpressionMode.Create;

		[SerializeField]
		private bool m_voidMode = false;

		[SerializeField]
		private bool m_autoRegisterMode = false;

		[SerializeField]
		private bool m_functionMode = false;

		[SerializeField]
		private int m_firstAvailablePort = 0;

		[SerializeField]
		private string m_uniqueName;

		[SerializeField]
		private bool m_generateUniqueName = true;

		private int m_markedToDelete = -1;
		private const float ButtonLayoutWidth = 15;

		private bool m_repopulateNameDictionary = true;
		private Dictionary<string, int> m_usedNames = new Dictionary<string, int>();

		private double m_lastTimeNameModified = 0;
		private bool m_nameModified = false;

		private double m_lastTimeCodeModified = 0;
		private bool m_codeModified = false;

		//private bool m_editPropertyNameMode = false;

		//Title editing 
		private bool m_isEditing;
		private bool m_stopEditing;
		private bool m_startEditing;
		private double m_clickTime;
		private double m_doubleClickTime = 0.3;
		private Rect m_titleClickArea;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, "In0" );
			m_foldoutValuesFlags.Add( true );
			m_foldoutValuesLabels.Add( "[0]" );
			m_variableQualifiers.Add( VariableQualifiers.In );
			AddOutputPort( WirePortDataType.FLOAT, "Out" );
			m_textLabelWidth = 97;
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			//m_customExpressionName = DefaultExpressionName;
			SetTitleText( m_customExpressionName );

			if( m_nodeAttribs != null )
				m_uniqueName = m_nodeAttribs.Name + OutputId;
			else
				m_uniqueName = "CustomExpression" + OutputId;
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			CheckPortConnection( portId );
		}

		public override void OnConnectedOutputNodeChanges( int portId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( portId, otherNodeId, otherPortId, name, type );
			CheckPortConnection( portId );
		}

		void CheckPortConnection( int portId )
		{
			if( portId == 0 && ( m_mode == CustomExpressionMode.Call || m_voidMode))
			{
				m_inputPorts[ 0 ].MatchPortToConnection();
				m_outputPorts[ 0 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
			}
		}

		public override void OnNodeLogicUpdate( DrawInfo drawInfo )
		{
			base.OnNodeLogicUpdate( drawInfo );
			if( m_nameModified )
			{
				if( ( EditorApplication.timeSinceStartup - m_lastTimeNameModified ) > MaxTimestamp )
				{
					m_nameModified = false;
					m_repopulateNameDictionary = true;
				}
			}

			if( m_repopulateNameDictionary )
			{
				m_repopulateNameDictionary = false;
				m_usedNames.Clear();
				for( int i = 0; i < m_inputPorts.Count; i++ )
				{
					m_usedNames.Add( m_inputPorts[ i ].Name, i );
				}
			}

			if( m_codeModified )
			{
				if( ( EditorApplication.timeSinceStartup - m_lastTimeCodeModified ) > MaxTimestamp )
				{
					m_codeModified = false;
					bool functionMode = m_code.Contains( ReturnHelper );
					if( functionMode != m_functionMode )
					{
						//if( m_functionMode )
						//{
						//    while( m_outputPorts.Count > 1 )
						//    {
						//        RemoveOutputPort( m_outputPorts.Count - 1 );
						//    }
						//}
						m_functionMode = functionMode;
						CheckCallMode();
					}
				}
			}
		}

		bool CheckCallMode()
		{
			if( m_functionMode && m_mode == CustomExpressionMode.Call )
			{
				Mode = CustomExpressionMode.Create;
				m_outputTypeIdx = ( AvailableOutputWireTypesStr.Length - 1 );
				m_outputPorts[ 0 ].ChangeType( AvailableOutputWireTypes[ m_outputTypeIdx ], false );
				m_voidMode = true;
				return true;
			}
			return false;
		}

		public override void Draw( DrawInfo drawInfo )
		{
			
			base.Draw( drawInfo );

			if( ContainerGraph.LodLevel <= ParentGraph.NodeLOD.LOD3 )
			{
				if( !m_isEditing && ( ( !ContainerGraph.ParentWindow.MouseInteracted && drawInfo.CurrentEventType == EventType.MouseDown && m_titleClickArea.Contains( drawInfo.MousePosition ) ) ) )
				{
					if( ( EditorApplication.timeSinceStartup - m_clickTime ) < m_doubleClickTime )
						m_startEditing = true;
					else
						GUI.FocusControl( null );
					m_clickTime = EditorApplication.timeSinceStartup;
				}
				else if( m_isEditing && ( ( drawInfo.CurrentEventType == EventType.MouseDown && !m_titleClickArea.Contains( drawInfo.MousePosition ) ) || !EditorGUIUtility.editingTextField ) )
				{
					m_stopEditing = true;
				}

				if( m_isEditing || m_startEditing )
				{
					EditorGUI.BeginChangeCheck();
					GUI.SetNextControlName( m_uniqueName );
					m_customExpressionName = EditorGUITextField( m_titleClickArea, string.Empty, m_customExpressionName, UIUtils.GetCustomStyle( CustomStyle.NodeTitle ) );
					if( EditorGUI.EndChangeCheck() )
					{
						SetTitleText( m_customExpressionName );
						m_sizeIsDirty = true;
						m_isDirty = true;
					}

					if( m_startEditing )
						EditorGUI.FocusTextInControl( m_uniqueName );
				}

				if( drawInfo.CurrentEventType == EventType.Repaint )
				{
					if( m_startEditing )
					{
						m_startEditing = false;
						m_isEditing = true;
					}

					if( m_stopEditing )
					{
						m_stopEditing = false;
						m_isEditing = false;
						GUI.FocusControl( null );
					}
				}
			}
		}

		public override void OnNodeLayout( DrawInfo drawInfo )
		{
			base.OnNodeLayout( drawInfo );
			m_titleClickArea = m_titlePos;
			m_titleClickArea.height = Constants.NODE_HEADER_HEIGHT;
		}

		public override void OnNodeRepaint( DrawInfo drawInfo )
		{
			base.OnNodeRepaint( drawInfo );
			if( !m_isVisible )
				return;

			// Fixed Title ( only renders when not editing )
			if( !m_isEditing && !m_startEditing && ContainerGraph.LodLevel <= ParentGraph.NodeLOD.LOD3 )
			{
				GUI.Label( m_titleClickArea, m_content, UIUtils.GetCustomStyle( CustomStyle.NodeTitle ) );
			}
		}

		public string GetFirstAvailableName()
		{
			string name = string.Empty;
			for( int i = 0; i < m_inputPorts.Count + 1; i++ )
			{
				name = DefaultInputName + i;
				if( !m_usedNames.ContainsKey( name ) )
				{
					return name;
				}
			}
			Debug.LogWarning( "Could not find valid name" );
			return string.Empty;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			NodeUtils.DrawPropertyGroup( ref m_propertiesFoldout, Constants.ParameterLabelStr, DrawBaseProperties );
			NodeUtils.DrawPropertyGroup( ref m_visibleInputsFoldout, InputsStr, DrawInputs, DrawAddRemoveInputs );

			EditorGUILayout.HelpBox( CustomExpressionInfo, MessageType.Info );
		}

		string WrapCodeInFunction( bool isTemplate, string functionName, bool expressionMode )
		{
			//Hack to be used util indent is properly used
			int currIndent = UIUtils.ShaderIndentLevel;
			UIUtils.ShaderIndentLevel = isTemplate ? 0 : 1;

			if( !isTemplate ) UIUtils.ShaderIndentLevel++;

			//string functionName = UIUtils.RemoveInvalidCharacters( m_customExpressionName );
			string returnType = ( m_mode == CustomExpressionMode.Call || m_voidMode ) ? "void" : UIUtils.PrecisionWirePortToCgType( m_currentPrecisionType, m_outputPorts[ 0 ].DataType );
			if( expressionMode )
				returnType = "inline " + returnType;

			string functionBody = UIUtils.ShaderIndentTabs + returnType + " " + functionName + "( ";
			int count = m_inputPorts.Count - m_firstAvailablePort;
			for( int i = 0; i < count; i++ )
			{
				int portIdx = i + m_firstAvailablePort;
				string qualifier = m_variableQualifiers[ i ] == VariableQualifiers.In ? string.Empty : UIUtils.QualifierToCg( m_variableQualifiers[ i ] ) + " ";
				functionBody += qualifier + UIUtils.PrecisionWirePortToCgType( m_currentPrecisionType, m_inputPorts[ portIdx ].DataType ) + " " + m_inputPorts[ portIdx ].Name;
				if( i < ( count - 1 ) )
				{
					functionBody += " , ";
				}
			}
			functionBody += " )\n" + UIUtils.ShaderIndentTabs + "{\n";
			UIUtils.ShaderIndentLevel++;
			{
				if( expressionMode )
					functionBody += UIUtils.ShaderIndentTabs + "return ";

				string[] codeLines = m_code.Split( IOUtils.LINE_TERMINATOR );
				for( int i = 0; i < codeLines.Length; i++ )
				{
					if( codeLines[ i ].Length > 0 )
					{
						functionBody += ( ( i == 0 && expressionMode ) ? string.Empty : UIUtils.ShaderIndentTabs ) + codeLines[ i ] + ( ( ( i == codeLines.Length - 1 ) && expressionMode ) ? string.Empty : "\n" );
					}
				}
				if( expressionMode )
					functionBody += ";\n";
			}
			UIUtils.ShaderIndentLevel--;

			functionBody += UIUtils.ShaderIndentTabs + "}\n";
			UIUtils.ShaderIndentLevel = currIndent;
			return functionBody;
		}

		void DrawBaseProperties()
		{
			EditorGUI.BeginChangeCheck();
			m_customExpressionName = EditorGUILayoutTextField( ExpressionNameLabel, m_customExpressionName );
			if( EditorGUI.EndChangeCheck() )
			{
				SetTitleText( m_customExpressionName );
			}
			
			EditorGUI.BeginChangeCheck();
			//m_mode = EditorGUILayoutToggle( FunctionCallMode, m_mode );
			Mode = (CustomExpressionMode) EditorGUILayoutEnumPopup( FunctionCallMode, m_mode );
			if( EditorGUI.EndChangeCheck() )
			{
				if( CheckCallMode() )
					UIUtils.ShowMessage( "Call Mode cannot have return over is code.\nFalling back to Create Mode" );
				SetupCallMode();
				RecalculateInOutOutputPorts();
			}

			EditorGUILayout.LabelField( CodeTitleStr );
			EditorGUI.BeginChangeCheck();
			{
				m_code = EditorGUILayoutTextArea( m_code, UIUtils.MainSkin.textArea );
			}
			if( EditorGUI.EndChangeCheck() )
			{
				m_codeModified = true;
				m_lastTimeCodeModified = EditorApplication.timeSinceStartup;
			}

			if( m_mode == CustomExpressionMode.Create )
			{
				DrawPrecisionProperty();
				m_generateUniqueName = EditorGUILayoutToggle( GenerateUniqueName, m_generateUniqueName );
				AutoRegisterMode = EditorGUILayoutToggle( AutoRegister, m_autoRegisterMode );

				EditorGUI.BeginChangeCheck();
				m_outputTypeIdx = EditorGUILayoutPopup( OutputTypeStr, m_outputTypeIdx, AvailableOutputWireTypesStr );
				if( EditorGUI.EndChangeCheck() )
				{
					bool oldVoidValue = m_voidMode;
					UpdateVoidMode();
					if( oldVoidValue != m_voidMode )
					{
						SetupCallMode();
						RecalculateInOutOutputPorts();
					}
					else
					{
						m_outputPorts[ 0 ].ChangeType( AvailableOutputWireTypes[ m_outputTypeIdx ], false );
					}
				}
			}
		}

		void UpdateVoidMode()
		{
			m_voidMode = ( m_outputTypeIdx == ( AvailableOutputWireTypesStr.Length - 1 ) );
		}

		void SetupCallMode()
		{
			if( m_mode == CustomExpressionMode.Call || m_voidMode )
			{
				if( m_firstAvailablePort != 1 )
				{
					m_firstAvailablePort = 1;
					AddInputPortAt( 0, WirePortDataType.FLOAT, false, DefaultInputName );
					m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
				}
			}
			else
			{
				if( m_firstAvailablePort != 0 )
				{
					m_firstAvailablePort = 0;
					if( m_inputPorts[ 0 ].IsConnected )
					{
						m_containerGraph.DeleteConnection( true, UniqueId, m_inputPorts[ 0 ].PortId, false, true );
					}
					DeleteInputPortByArrayIdx( 0 );
					m_outputPorts[ 0 ].ChangeType( AvailableOutputWireTypes[ m_outputTypeIdx ], false );
				}
			}
		}

		void DrawAddRemoveInputs()
		{
			if( m_inputPorts.Count == m_firstAvailablePort )
				m_visibleInputsFoldout = false;

			// Add new port
			if( GUILayoutButton( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ButtonLayoutWidth ) ) )
			{
				AddPortAt( m_inputPorts.Count );
				m_visibleInputsFoldout = true;
				EditorGUI.FocusTextInControl( null );
			}

			//Remove port
			if( GUILayoutButton( string.Empty, UIUtils.MinusStyle, GUILayout.Width( ButtonLayoutWidth ) ) )
			{
				RemovePortAt( m_inputPorts.Count - 1 );
				EditorGUI.FocusTextInControl( null );
			}
		}

		void DrawInputs()
		{
			int count = m_inputPorts.Count - m_firstAvailablePort;
			for( int i = 0; i < count; i++ )
			{
				int portIdx = i + m_firstAvailablePort;
				m_foldoutValuesFlags[ i ] = EditorGUILayoutFoldout( m_foldoutValuesFlags[ i ], m_foldoutValuesLabels[ i ] + " - " + m_inputPorts[ portIdx ].Name );

				if( m_foldoutValuesFlags[ i ] )
				{
					EditorGUI.indentLevel += 1;

					//Qualifier
					// bool guiEnabled = GUI.enabled;
					// GUI.enabled = m_functionMode;

					VariableQualifiers newQualifier = (VariableQualifiers)EditorGUILayoutPopup( InputQualifierStr, (int)m_variableQualifiers[ i ], QualifiersStr );
					if( newQualifier != m_variableQualifiers[ i ] )
					{
						VariableQualifiers oldQualifier = m_variableQualifiers[ i ];
						m_variableQualifiers[ i ] = newQualifier;
						if( newQualifier == VariableQualifiers.In )
						{
							RemoveOutputPort( CreateOutputId( m_inputPorts[ portIdx ].PortId ), false );
						}
						else if( oldQualifier == VariableQualifiers.In )
						{
							AddOutputPort( m_inputPorts[ portIdx ].DataType, m_inputPorts[ portIdx ].Name, CreateOutputId( m_inputPorts[ portIdx ].PortId ) );
						}
						RecalculateInOutOutputPorts();
					}

					//GUI.enabled = guiEnabled;

					// Type
					int typeIdx = WireToIdx[ m_inputPorts[ portIdx ].DataType ];
					EditorGUI.BeginChangeCheck();
					{
						typeIdx = EditorGUILayoutPopup( InputTypeStr, typeIdx, AvailableWireTypesStr );
					}

					if( EditorGUI.EndChangeCheck() )
					{
						m_inputPorts[ portIdx ].ChangeType( AvailableWireTypes[ typeIdx ], false );
						if( m_variableQualifiers[ i ] != VariableQualifiers.In )
						{
							OutputPort currOutPort = GetOutputPortByUniqueId( CreateOutputId( m_inputPorts[ portIdx ].PortId ) );
							currOutPort.ChangeType( AvailableWireTypes[ typeIdx ], false );
						}
					}

					//Name
					EditorGUI.BeginChangeCheck();
					{
						m_inputPorts[ portIdx ].Name = EditorGUILayoutTextField( InputNameStr, m_inputPorts[ portIdx ].Name );
					}
					if( EditorGUI.EndChangeCheck() )
					{
						m_nameModified = true;
						m_lastTimeNameModified = EditorApplication.timeSinceStartup;
						m_inputPorts[ portIdx ].Name = UIUtils.RemoveInvalidCharacters( m_inputPorts[ portIdx ].Name );
						if( string.IsNullOrEmpty( m_inputPorts[ portIdx ].Name ) )
						{
							m_inputPorts[ portIdx ].Name = DefaultInputName + i;
						}
					}

					// Port Data
					if( !m_inputPorts[ portIdx ].IsConnected )
					{
						m_inputPorts[ portIdx ].ShowInternalData( this, true, InputValueStr );
					}

					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label( " " );
						// Add new port
						if( GUILayoutButton( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ButtonLayoutWidth ) ) )
						{
							AddPortAt( portIdx );
							EditorGUI.FocusTextInControl( null );
						}

						//Remove port
						if( GUILayoutButton( string.Empty, UIUtils.MinusStyle, GUILayout.Width( ButtonLayoutWidth ) ) )
						{
							m_markedToDelete = portIdx;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.indentLevel -= 1;
				}
			}

			if( m_markedToDelete > -1 )
			{
				RemovePortAt( m_markedToDelete );
				m_markedToDelete = -1;
				EditorGUI.FocusTextInControl( null );
			}
		}

		void RecalculateInOutOutputPorts()
		{
			m_outputPorts.Sort( ( x, y ) => x.PortId.CompareTo( y.PortId ) );

			m_outputPortsDict.Clear();
			int count = m_inputPorts.Count - m_firstAvailablePort;
			int outputId = 1;
			for( int i = 0; i < count; i++ )
			{
				int idx = i + m_firstAvailablePort;
				if( m_variableQualifiers[ i ] != VariableQualifiers.In )
				{
					m_outputPorts[ outputId ].ChangeProperties( m_inputPorts[ idx ].Name, m_inputPorts[ idx ].DataType, false );
					m_outputPorts[ outputId ].ChangePortId( CreateOutputId( m_inputPorts[ idx ].PortId ) );
					outputId++;
				}
			}

			int outCount = m_outputPorts.Count;
			for( int i = 0; i < outCount; i++ )
			{
				m_outputPortsDict.Add( m_outputPorts[ i ].PortId, m_outputPorts[ i ] );
			}
		}

		void AddPortAt( int idx )
		{
			AddInputPortAt( idx, WirePortDataType.FLOAT, false, GetFirstAvailableName() );

			m_foldoutValuesFlags.Add( true );
			m_foldoutValuesLabels.Add( "[" + idx + "]" );
			m_variableQualifiers.Add( VariableQualifiers.In );
			m_repopulateNameDictionary = true;
		}

		void RemovePortAt( int idx )
		{
			if( m_inputPorts.Count > m_firstAvailablePort )
			{
				bool recalculateOutputs = false;
				int varIdx = idx - m_firstAvailablePort;
				if( m_variableQualifiers[ varIdx ] != VariableQualifiers.In )
				{
					RemoveOutputPort( CreateOutputId( m_inputPorts[ idx ].PortId ), false );
					recalculateOutputs = true;
				}

				DeleteInputPortByArrayIdx( idx );

				m_foldoutValuesFlags.RemoveAt( varIdx );
				m_foldoutValuesLabels.RemoveAt( varIdx );
				m_variableQualifiers.RemoveAt( varIdx );

				m_repopulateNameDictionary = true;
				if( recalculateOutputs )
					RecalculateInOutOutputPorts();
			}
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			m_repopulateNameDictionary = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( string.IsNullOrEmpty( m_code ) )
			{
				UIUtils.ShowMessage( "Custom Expression need to have code associated", MessageSeverity.Warning );
				return "0";
			}

			m_code = m_code.Replace( "\r\n", "\n" );

			bool codeContainsReturn = m_code.Contains( ReturnHelper );
			if( !codeContainsReturn && outputId != 0 && m_mode == CustomExpressionMode.Create && !m_voidMode)
			{
				UIUtils.ShowMessage( "Attempting to get value from inexisting inout/out variable", MessageSeverity.Warning );
				return "0";
			}

			OutputPort outputPort = GetOutputPortByUniqueId( outputId );
			if( outputPort.IsLocalValue( dataCollector.PortCategory ) )
				return outputPort.LocalValue( dataCollector.PortCategory );

			string expressionName = UIUtils.RemoveInvalidCharacters( m_customExpressionName );
			string localVarName = "local" + expressionName;

			if( m_generateUniqueName )
			{
				expressionName += OutputId;
			}
			localVarName += OutputId;

			int count = m_inputPorts.Count;
			if( count > 0 )
			{
				if( m_mode == CustomExpressionMode.Call || m_voidMode )
				{
					string mainData = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
					RegisterLocalVariable( 0, string.Format( Constants.CodeWrapper, mainData ), ref dataCollector, localVarName );
				}

				if( codeContainsReturn )
				{
					string function = WrapCodeInFunction( dataCollector.IsTemplate, expressionName, false );

					string functionCall = expressionName + "( ";
					for( int i = m_firstAvailablePort; i < count; i++ )
					{
						string inputPortLocalVar = m_inputPorts[ i ].Name + OutputId;
						string result = m_inputPorts[ i ].GeneratePortInstructions( ref dataCollector );
						dataCollector.AddLocalVariable( UniqueId, m_currentPrecisionType, m_inputPorts[ i ].DataType, inputPortLocalVar, result );
						int idx = i - m_firstAvailablePort;
						if( m_variableQualifiers[ idx ] != VariableQualifiers.In )
						{
							OutputPort currOutputPort = GetOutputPortByUniqueId( CreateOutputId( m_inputPorts[ i ].PortId ) );
							currOutputPort.SetLocalValue( inputPortLocalVar, dataCollector.PortCategory );
						}
						functionCall += inputPortLocalVar;
						if( i < ( count - 1 ) )
						{
							functionCall += " , ";
						}
					}
					functionCall += " )";

					if( m_mode == CustomExpressionMode.Call || m_voidMode )
					{
						dataCollector.AddLocalVariable( 0, functionCall + ";", true );
					}
					else
					{
						RegisterLocalVariable( 0, functionCall, ref dataCollector, localVarName );
					}

					dataCollector.AddFunction( expressionName, function );
				}
				else
				{
					string localCode = m_code;
					if( m_mode == CustomExpressionMode.Call || m_voidMode )
					{
						for( int i = m_firstAvailablePort; i < count; i++ )
						{
							string inputPortLocalVar = m_inputPorts[ i ].Name + OutputId;
							localCode = localCode.Replace( m_inputPorts[ i ].Name, inputPortLocalVar );

							if( m_inputPorts[ i ].IsConnected )
							{
								string result = m_inputPorts[ i ].GenerateShaderForOutput( ref dataCollector, m_inputPorts[ i ].DataType, true, true );
								dataCollector.AddLocalVariable( UniqueId, m_currentPrecisionType, m_inputPorts[ i ].DataType, inputPortLocalVar, result );
							}
							else
							{
								dataCollector.AddLocalVariable( UniqueId, m_currentPrecisionType, m_inputPorts[ i ].DataType, inputPortLocalVar, m_inputPorts[ i ].WrappedInternalData );
							}
							int idx = i - m_firstAvailablePort;
							if( m_variableQualifiers[ idx ] != VariableQualifiers.In )
							{
								OutputPort currOutputPort = GetOutputPortByUniqueId( CreateOutputId( m_inputPorts[ i ].PortId ) );
								currOutputPort.SetLocalValue( inputPortLocalVar, dataCollector.PortCategory );
							}
						}
						string[] codeLines = localCode.Split( '\n' );
						for( int codeIdx = 0; codeIdx < codeLines.Length; codeIdx++ )
						{
							dataCollector.AddLocalVariable( 0, codeLines[ codeIdx ], true );
						}
					}
					else
					{
						string function = WrapCodeInFunction( dataCollector.IsTemplate, expressionName, true );

						string functionCall = expressionName + "( ";
						for( int i = m_firstAvailablePort; i < count; i++ )
						{
							string inputPortLocalVar = m_inputPorts[ i ].Name + OutputId;
							string result = m_inputPorts[ i ].GeneratePortInstructions( ref dataCollector );
							dataCollector.AddLocalVariable( UniqueId, m_currentPrecisionType, m_inputPorts[ i ].DataType, inputPortLocalVar, result );
							int idx = i - m_firstAvailablePort;
							if( m_variableQualifiers[ idx ] != VariableQualifiers.In )
							{
								OutputPort currOutputPort = GetOutputPortByUniqueId( CreateOutputId( m_inputPorts[ i ].PortId ) );
								currOutputPort.SetLocalValue( inputPortLocalVar, dataCollector.PortCategory );
							}
							functionCall += inputPortLocalVar;
							if( i < ( count - 1 ) )
							{
								functionCall += " , ";
							}
						}
						functionCall += " )";
						RegisterLocalVariable( 0, functionCall, ref dataCollector, localVarName );
						dataCollector.AddFunction( expressionName, function );
					}
				}

				return outputPort.LocalValue( dataCollector.PortCategory );
			}
			else
			{
				if( m_code.Contains( ReturnHelper ) )
				{
					string function = WrapCodeInFunction( dataCollector.IsTemplate, expressionName, false );
					dataCollector.AddFunction( expressionName, function );
					string functionCall = expressionName + "()";
					RegisterLocalVariable( 0, functionCall, ref dataCollector, localVarName );
				}
				else
				{
					RegisterLocalVariable( 0, string.Format( Constants.CodeWrapper, m_code ), ref dataCollector, localVarName );
				}

				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
			}
		}

		//public override void OnNodeDoubleClicked( Vector2 currentMousePos2D )
		//{
		//    if( currentMousePos2D.y - m_globalPosition.y > Constants.NODE_HEADER_HEIGHT + Constants.NODE_HEADER_EXTRA_HEIGHT )
		//    {
		//        ContainerGraph.ParentWindow.ParametersWindow.IsMaximized = !ContainerGraph.ParentWindow.ParametersWindow.IsMaximized;
		//    }
		//    else
		//    {
		//        m_editPropertyNameMode = true;
		//        GUI.FocusControl( m_uniqueName );
		//        TextEditor te = (TextEditor)GUIUtility.GetStateObject( typeof( TextEditor ), GUIUtility.keyboardControl );
		//        if( te != null )
		//        {
		//            te.SelectAll();
		//        }
		//    }
		//}

		//public override void OnNodeSelected( bool value )
		//{
		//    base.OnNodeSelected( value );
		//    if( !value )
		//        m_editPropertyNameMode = false;
		//}

		//public override void DrawTitle( Rect titlePos )
		//{
		//    if( m_editPropertyNameMode )
		//    {
		//        titlePos.height = Constants.NODE_HEADER_HEIGHT;
		//        EditorGUI.BeginChangeCheck();
		//        GUI.SetNextControlName( m_uniqueName );
		//        m_customExpressionName = GUITextField( titlePos, m_customExpressionName, UIUtils.GetCustomStyle( CustomStyle.NodeTitle ) );
		//        if( EditorGUI.EndChangeCheck() )
		//        {
		//            SetTitleText( m_customExpressionName );
		//        }

		//        if( Event.current.isKey && ( Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter ) )
		//        {
		//            m_editPropertyNameMode = false;
		//            GUIUtility.keyboardControl = 0;
		//        }
		//    }
		//    else
		//    {
		//        base.DrawTitle( titlePos );
		//    }
		//}

		int CreateOutputId( int inputId )
		{
			return ( inputId + 1 );
		}

		int CreateInputId( int outputId )
		{
			return outputId - 1;
		}

		void UpdateOutputPorts()
		{
			int count = m_inputPorts.Count - m_firstAvailablePort;
			for( int i = 0; i < count; i++ )
			{
				if( m_variableQualifiers[ i ] != VariableQualifiers.In )
				{
					int portIdx = i + m_firstAvailablePort;
					AddOutputPort( m_inputPorts[ portIdx ].DataType, m_inputPorts[ portIdx ].Name, CreateOutputId( m_inputPorts[ portIdx ].PortId ) );
				}
			}
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			// This node is, by default, created with one input port 
			base.ReadFromString( ref nodeParams );
			m_code = GetCurrentParam( ref nodeParams );
			m_code = m_code.Replace( LineFeedSeparator, '\n' );
			m_code = m_code.Replace( SemiColonSeparator, ';' );
			m_outputTypeIdx = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			if( m_outputTypeIdx >= AvailableWireTypes.Length )
			{
				UIUtils.ShowMessage( "Sampler types were removed as a valid output custom expression type" );
				m_outputTypeIdx = 1;
			}
			UpdateVoidMode();
			m_outputPorts[ 0 ].ChangeType( AvailableWireTypes[ m_outputTypeIdx ], false );

			if( UIUtils.CurrentShaderVersion() > 12001 )
			{
				bool mode = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
				m_mode = mode ? CustomExpressionMode.Call : CustomExpressionMode.Create;
				if( m_mode == CustomExpressionMode.Call || m_voidMode )
				{
					m_firstAvailablePort = 1;
					AddInputPortAt( 0, WirePortDataType.FLOAT, false, DefaultInputName );
				}
			}

			int count = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			if( count == 0 )
			{
				DeleteInputPortByArrayIdx( 0 );
				m_foldoutValuesLabels.Clear();
				m_variableQualifiers.Clear();
			}
			else
			{
				for( int i = 0; i < count; i++ )
				{
					bool foldoutValue = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
					string name = GetCurrentParam( ref nodeParams );
					WirePortDataType type = (WirePortDataType)Enum.Parse( typeof( WirePortDataType ), GetCurrentParam( ref nodeParams ) );
					string internalData = GetCurrentParam( ref nodeParams );
					VariableQualifiers qualifier = VariableQualifiers.In;
					if( UIUtils.CurrentShaderVersion() > 12001 )
					{
						qualifier = (VariableQualifiers)Enum.Parse( typeof( VariableQualifiers ), GetCurrentParam( ref nodeParams ) );
					}

					int portIdx = i + m_firstAvailablePort;
					if( i == 0 )
					{
						m_inputPorts[ portIdx ].ChangeProperties( name, type, false );
						m_variableQualifiers[ 0 ] = qualifier;
						m_foldoutValuesFlags[ 0 ] = foldoutValue;
					}
					else
					{
						m_foldoutValuesLabels.Add( "[" + i + "]" );
						m_variableQualifiers.Add( qualifier );
						m_foldoutValuesFlags.Add( foldoutValue );
						AddInputPort( type, false, name );
					}
					m_inputPorts[ i ].InternalData = internalData;
				}
			}

			if( UIUtils.CurrentShaderVersion() > 7205 )
			{
				m_customExpressionName = GetCurrentParam( ref nodeParams );
				SetTitleText( m_customExpressionName );
			}

			if( UIUtils.CurrentShaderVersion() > 14401 )
			{
				m_generateUniqueName = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			}

			if( UIUtils.CurrentShaderVersion() > 15102 )
			{
				m_autoRegisterMode = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			}

			if( m_autoRegisterMode )
			{
				m_containerGraph.CustomExpressionOnFunctionMode.AddNode( this ); 
			}
			UpdateOutputPorts();

			m_repopulateNameDictionary = true;
			m_functionMode = m_code.Contains( ReturnHelper );
			CheckCallMode();
			
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );

			m_code = m_code.Replace( "\r\n", "\n" );

			string parsedCode = m_code.Replace( '\n', LineFeedSeparator );
			parsedCode = parsedCode.Replace( ';', SemiColonSeparator );

			IOUtils.AddFieldValueToString( ref nodeInfo, parsedCode );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_outputTypeIdx );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_mode == CustomExpressionMode.Call );

			int count = m_inputPorts.Count - m_firstAvailablePort;
			IOUtils.AddFieldValueToString( ref nodeInfo, count );
			for( int i = 0; i < count; i++ )
			{
				int portIdx = m_firstAvailablePort + i;
				IOUtils.AddFieldValueToString( ref nodeInfo, m_foldoutValuesFlags[ i ] );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_inputPorts[ portIdx ].Name );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_inputPorts[ portIdx ].DataType );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_inputPorts[ portIdx ].InternalData );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_variableQualifiers[ i ] );
			}
			IOUtils.AddFieldValueToString( ref nodeInfo, m_customExpressionName );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_generateUniqueName );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_autoRegisterMode );
		}

		public override void Destroy()
		{
			base.Destroy();
			if( m_autoRegisterMode )
			{
				m_containerGraph.CustomExpressionOnFunctionMode.RemoveNode( this );
			}
		}

		public CustomExpressionMode Mode
		{
			get { return m_mode; }
			set
			{
				if( m_mode != value )
				{
					m_mode = value;
					if( m_mode == CustomExpressionMode.Call )
					{
						AutoRegisterMode = false;
						m_generateUniqueName = false;
					}
				}
				
			}
		}
		public bool AutoRegisterMode
		{
			get { return m_autoRegisterMode; }
			set
			{
				if( value != m_autoRegisterMode )
				{
					m_autoRegisterMode = value;
					if( m_autoRegisterMode )
					{
						m_containerGraph.CustomExpressionOnFunctionMode.AddNode( this );
					}
					else
					{
						m_containerGraph.CustomExpressionOnFunctionMode.RemoveNode( this );
					}
				}
			}
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			int portCount = m_inputPorts.Count;
			for( int i = 0; i < portCount; i++ )
			{
				if( m_inputPorts[ i ].DataType == WirePortDataType.COLOR )
				{
					m_inputPorts[ i ].ChangeType( WirePortDataType.FLOAT4, false ); ;
				}
			}
		}

		public string EncapsulatedCode( bool isTemplate )
		{
			string functionName = UIUtils.RemoveInvalidCharacters( m_customExpressionName );
			if( m_generateUniqueName )
			{
				functionName += OutputId;
			}
			return WrapCodeInFunction( isTemplate, functionName, false );
		}

	}
}
