using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{

    [Serializable]
    public sealed class TemplatesStencilBufferModule : TemplateModuleParent
    {
        private const string FoldoutLabelStr = " Stencil Buffer";
        private GUIContent ReferenceValueContent = new GUIContent( "Reference", "The value to be compared against (if Comparison is anything else than always) and/or the value to be written to the buffer (if either Pass, Fail or ZFail is set to replace)" );
        private GUIContent ReadMaskContent = new GUIContent( "Read Mask", "An 8 bit mask as an 0-255 integer, used when comparing the reference value with the contents of the buffer (referenceValue & readMask) comparisonFunction (stencilBufferValue & readMask)" );
        private GUIContent WriteMaskContent = new GUIContent( "Write Mask", "An 8 bit mask as an 0-255 integer, used when writing to the buffer" );
        private const string ComparisonStr = "Comparison";
        private const string PassStr = "Pass";
        private const string FailStr = "Fail";
        private const string ZFailStr = "ZFail";

        private const string ComparisonFrontStr = "Comp. Front";
        private const string PassFrontStr = "Pass Front";
        private const string FailFrontStr = "Fail Front";
        private const string ZFailFrontStr = "ZFail Front";

        private const string ComparisonBackStr = "Comp. Back";
        private const string PassBackStr = "Pass Back";
        private const string FailBackStr = "Fail Back";
        private const string ZFailBackStr = "ZFail Back";

        private Dictionary<string, int> m_comparisonDict = new Dictionary<string, int>();
        private Dictionary<string, int> m_stencilOpsDict = new Dictionary<string, int>();

        [SerializeField]
        private int m_reference;

        // Read Mask
        private const int ReadMaskDefaultValue = 255;
        [SerializeField]
        private int m_readMask = ReadMaskDefaultValue;

        //Write Mask
        private const int WriteMaskDefaultValue = 255;
        [SerializeField]
        private int m_writeMask = WriteMaskDefaultValue;

        //Comparison Function
        private const int ComparisonDefaultValue = 0;
        [SerializeField]
        private int m_comparisonFunctionIdx = ComparisonDefaultValue;

        [SerializeField]
        private int m_comparisonFunctionBackIdx = ComparisonDefaultValue;

        //Pass Stencil Op
        private const int PassStencilOpDefaultValue = 0;
        [SerializeField]
        private int m_passStencilOpIdx = PassStencilOpDefaultValue;

        [SerializeField]
        private int m_passStencilOpBackIdx = PassStencilOpDefaultValue;

        //Fail Stencil Op 
        private const int FailStencilOpDefaultValue = 0;

        [SerializeField]
        private int m_failStencilOpIdx = FailStencilOpDefaultValue;

        [SerializeField]
        private int m_failStencilOpBackIdx = FailStencilOpDefaultValue;

        //ZFail Stencil Op
        private const int ZFailStencilOpDefaultValue = 0;
        [SerializeField]
        private int m_zFailStencilOpIdx = ZFailStencilOpDefaultValue;

        [SerializeField]
        private int m_zFailStencilOpBackIdx = ZFailStencilOpDefaultValue;

        public TemplatesStencilBufferModule() : base("Stencil Buffer")
        {
            for( int i = 0; i < StencilBufferOpHelper.StencilComparisonValues.Length; i++ )
            {
                m_comparisonDict.Add( StencilBufferOpHelper.StencilComparisonValues[ i ].ToLower(), i );
            }

            for( int i = 0; i < StencilBufferOpHelper.StencilOpsValues.Length; i++ )
            {
                m_stencilOpsDict.Add( StencilBufferOpHelper.StencilOpsValues[ i ].ToLower(), i );
            }
        }

		public void CopyFrom( TemplatesStencilBufferModule other )
		{
			m_reference = other.Reference;
			m_readMask = other.ReadMask;
			m_writeMask = other.WriteMask;
			m_comparisonFunctionIdx = other.ComparisonFunctionIdx;
			m_comparisonFunctionBackIdx = other.ComparisonFunctionBackIdx;
			m_passStencilOpIdx = other.PassStencilOpIdx;
			m_passStencilOpBackIdx = other.PassStencilOpBackIdx;
			m_failStencilOpIdx = other.FailStencilOpIdx;
			m_failStencilOpBackIdx = other.FailStencilOpBackIdx;
			m_zFailStencilOpIdx = other.ZFailStencilOpIdx;
			m_zFailStencilOpBackIdx = other.ZFailStencilOpBackIdx;
		}

        public void ConfigureFromTemplateData( TemplateStencilData stencilData )
        {
			bool newValidData = ( stencilData.DataCheck == TemplateDataCheck.Valid );
			if( newValidData && m_validData != newValidData )
			{
				m_reference = stencilData.Reference;
				m_readMask = stencilData.ReadMask;
				m_writeMask = stencilData.WriteMask;

				if( !string.IsNullOrEmpty( stencilData.ComparisonFront ) )
				{
					m_comparisonFunctionIdx = m_comparisonDict[ stencilData.ComparisonFront.ToLower() ];
				}
				else
				{
					m_comparisonFunctionIdx = m_comparisonDict[ "always" ];
				}

				if( !string.IsNullOrEmpty( stencilData.PassFront ) )
				{
					m_passStencilOpIdx = m_stencilOpsDict[ stencilData.PassFront.ToLower() ];
				}
				else
				{
					m_passStencilOpIdx = m_stencilOpsDict[ "keep" ];
				}

				if( !string.IsNullOrEmpty( stencilData.FailFront ) )
				{
					m_failStencilOpIdx = m_stencilOpsDict[ stencilData.FailFront.ToLower() ];
				}
				else
				{
					m_failStencilOpIdx = m_stencilOpsDict[ "keep" ];
				}

				if( !string.IsNullOrEmpty( stencilData.ZFailFront ) )
				{
					m_zFailStencilOpIdx = m_stencilOpsDict[ stencilData.ZFailFront.ToLower() ];
				}
				else
				{
					m_zFailStencilOpIdx = m_stencilOpsDict[ "keep" ];
				}

				if( !string.IsNullOrEmpty( stencilData.ComparisonBack ) )
				{
					m_comparisonFunctionBackIdx = m_comparisonDict[ stencilData.ComparisonBack.ToLower() ];
				}
				else
				{
					m_comparisonFunctionBackIdx = m_comparisonDict[ "always" ];
				}

				if( !string.IsNullOrEmpty( stencilData.PassBack ) )
				{
					m_passStencilOpBackIdx = m_stencilOpsDict[ stencilData.PassBack.ToLower() ];
				}
				else
				{
					m_passStencilOpBackIdx = m_stencilOpsDict[ "keep" ];
				}

				if( !string.IsNullOrEmpty( stencilData.FailBack ) )
				{
					m_failStencilOpBackIdx = m_stencilOpsDict[ stencilData.FailBack.ToLower() ];
				}
				else
				{
					m_failStencilOpBackIdx = m_stencilOpsDict[ "keep" ];
				}

				if( !string.IsNullOrEmpty( stencilData.ZFailBack ) )
				{
					m_zFailStencilOpBackIdx = m_stencilOpsDict[ stencilData.ZFailBack.ToLower() ];
				}
				else
				{
					m_zFailStencilOpBackIdx = m_stencilOpsDict[ "keep" ];
				}
			}
			m_validData = newValidData;
		}

        public string CreateStencilOp( CullMode cullMode )
        {
            string result = "Stencil\n{\n";
            result += string.Format( "\tRef {0}\n", m_reference );
            if( m_readMask != ReadMaskDefaultValue )
            {
                result += string.Format( "\tReadMask {0}\n", m_readMask );
            }

            if( m_writeMask != WriteMaskDefaultValue )
            {
                result += string.Format( "\tWriteMask {0}\n", m_writeMask );
            }

            if( cullMode == CullMode.Off &&
               ( m_comparisonFunctionBackIdx != ComparisonDefaultValue ||
                m_passStencilOpBackIdx != PassStencilOpDefaultValue ||
                m_failStencilOpBackIdx != FailStencilOpDefaultValue ||
                m_zFailStencilOpBackIdx != ZFailStencilOpDefaultValue ) )
            {
                if( m_comparisonFunctionIdx != ComparisonDefaultValue )
                    result += string.Format( "\tCompFront {0}\n", StencilBufferOpHelper.StencilComparisonValues[ m_comparisonFunctionIdx ] );
                if( m_passStencilOpIdx != PassStencilOpDefaultValue )
                    result += string.Format( "\tPassFront {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_passStencilOpIdx ] );
                if( m_failStencilOpIdx != FailStencilOpDefaultValue )
                    result += string.Format( "\tFailFront {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_failStencilOpIdx ] );
                if( m_zFailStencilOpIdx != ZFailStencilOpDefaultValue )
                    result += string.Format( "\tZFailFront {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_zFailStencilOpIdx ] );

                if( m_comparisonFunctionBackIdx != ComparisonDefaultValue )
                    result += string.Format( "\tCompBack {0}\n", StencilBufferOpHelper.StencilComparisonValues[ m_comparisonFunctionBackIdx ] );
                if( m_passStencilOpBackIdx != PassStencilOpDefaultValue )
                    result += string.Format( "\tPassBack {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_passStencilOpBackIdx ] );
                if( m_failStencilOpBackIdx != FailStencilOpDefaultValue )
                    result += string.Format( "\tFailBack {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_failStencilOpBackIdx ] );
                if( m_zFailStencilOpBackIdx != ZFailStencilOpDefaultValue )
                    result += string.Format( "\tZFailBack {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_zFailStencilOpBackIdx ] );
            }
            else
            {
                if( m_comparisonFunctionIdx != ComparisonDefaultValue )
                    result += string.Format( "\tComp {0}\n", StencilBufferOpHelper.StencilComparisonValues[ m_comparisonFunctionIdx ] );
                if( m_passStencilOpIdx != PassStencilOpDefaultValue )
                    result += string.Format( "\tPass {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_passStencilOpIdx ] );
                if( m_failStencilOpIdx != FailStencilOpDefaultValue )
                    result += string.Format( "\tFail {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_failStencilOpIdx ] );
                if( m_zFailStencilOpIdx != ZFailStencilOpDefaultValue )
                    result += string.Format( "\tZFail {0}\n", StencilBufferOpHelper.StencilOpsValues[ m_zFailStencilOpIdx ] );
            }

            result += "}\n";
            return result;
        }

        public override void ShowUnreadableDataMessage()
        {
            NodeUtils.DrawPropertyGroup( ref m_foldoutValue, FoldoutLabelStr, base.ShowUnreadableDataMessage );
        }

        public void Draw( UndoParentNode owner, CullMode cullMode , bool style = true )
        {
			if( style )
			{
				NodeUtils.DrawPropertyGroup( ref m_foldoutValue, FoldoutLabelStr, () =>
				{
					DrawBlock( owner, cullMode );
				} );
			}
			else
			{
				NodeUtils.DrawNestedPropertyGroup( ref m_foldoutValue, FoldoutLabelStr, () =>
				{
					DrawBlock( owner, cullMode );
				} );
			}
        }

		void DrawBlock( UndoParentNode owner, CullMode cullMode )
		{
			EditorGUI.BeginChangeCheck();
			{
				m_reference = owner.EditorGUILayoutIntSlider( ReferenceValueContent, m_reference, 0, 255 );
				m_readMask = owner.EditorGUILayoutIntSlider( ReadMaskContent, m_readMask, 0, 255 );
				m_writeMask = owner.EditorGUILayoutIntSlider( WriteMaskContent, m_writeMask, 0, 255 );
				if( cullMode == CullMode.Off )
				{
					m_comparisonFunctionIdx = owner.EditorGUILayoutPopup( ComparisonFrontStr, m_comparisonFunctionIdx, StencilBufferOpHelper.StencilComparisonLabels );
					m_passStencilOpIdx = owner.EditorGUILayoutPopup( PassFrontStr, m_passStencilOpIdx, StencilBufferOpHelper.StencilOpsLabels );
					m_failStencilOpIdx = owner.EditorGUILayoutPopup( FailFrontStr, m_failStencilOpIdx, StencilBufferOpHelper.StencilOpsLabels );
					m_zFailStencilOpIdx = owner.EditorGUILayoutPopup( ZFailFrontStr, m_zFailStencilOpIdx, StencilBufferOpHelper.StencilOpsLabels );
					EditorGUILayout.Separator();
					m_comparisonFunctionBackIdx = owner.EditorGUILayoutPopup( ComparisonBackStr, m_comparisonFunctionBackIdx, StencilBufferOpHelper.StencilComparisonLabels );
					m_passStencilOpBackIdx = owner.EditorGUILayoutPopup( PassBackStr, m_passStencilOpBackIdx, StencilBufferOpHelper.StencilOpsLabels );
					m_failStencilOpBackIdx = owner.EditorGUILayoutPopup( FailBackStr, m_failStencilOpBackIdx, StencilBufferOpHelper.StencilOpsLabels );
					m_zFailStencilOpBackIdx = owner.EditorGUILayoutPopup( ZFailBackStr, m_zFailStencilOpBackIdx, StencilBufferOpHelper.StencilOpsLabels );
				}
				else
				{
					m_comparisonFunctionIdx = owner.EditorGUILayoutPopup( ComparisonStr, m_comparisonFunctionIdx, StencilBufferOpHelper.StencilComparisonLabels );
					m_passStencilOpIdx = owner.EditorGUILayoutPopup( PassStr, m_passStencilOpIdx, StencilBufferOpHelper.StencilOpsLabels );
					m_failStencilOpIdx = owner.EditorGUILayoutPopup( FailStr, m_failStencilOpIdx, StencilBufferOpHelper.StencilOpsLabels );
					m_zFailStencilOpIdx = owner.EditorGUILayoutPopup( ZFailStr, m_zFailStencilOpIdx, StencilBufferOpHelper.StencilOpsLabels );
				}
			}
			if( EditorGUI.EndChangeCheck() )
			{
				m_isDirty = true;
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
				m_reference = Convert.ToInt32( nodeParams[ index++ ] );
				m_readMask = Convert.ToInt32( nodeParams[ index++ ] );
				m_writeMask = Convert.ToInt32( nodeParams[ index++ ] );
				m_comparisonFunctionIdx = Convert.ToInt32( nodeParams[ index++ ] );
				m_passStencilOpIdx = Convert.ToInt32( nodeParams[ index++ ] );
				m_failStencilOpIdx = Convert.ToInt32( nodeParams[ index++ ] );
				m_zFailStencilOpIdx = Convert.ToInt32( nodeParams[ index++ ] );
				m_comparisonFunctionBackIdx = Convert.ToInt32( nodeParams[ index++ ] );
				m_passStencilOpBackIdx = Convert.ToInt32( nodeParams[ index++ ] );
				m_failStencilOpBackIdx = Convert.ToInt32( nodeParams[ index++ ] );
				m_zFailStencilOpBackIdx = Convert.ToInt32( nodeParams[ index++ ] );
			}
        }

        public override void WriteToString( ref string nodeInfo )
        {
			IOUtils.AddFieldValueToString( ref nodeInfo, m_validData );
			if( m_validData )
			{
				IOUtils.AddFieldValueToString( ref nodeInfo, m_reference );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_readMask );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_writeMask );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_comparisonFunctionIdx );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_passStencilOpIdx );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_failStencilOpIdx );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_zFailStencilOpIdx );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_comparisonFunctionBackIdx );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_passStencilOpBackIdx );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_failStencilOpBackIdx );
				IOUtils.AddFieldValueToString( ref nodeInfo, m_zFailStencilOpBackIdx );
			}
        }

        public override void Destroy()
        {
            m_comparisonDict.Clear();
            m_comparisonDict = null;

            m_stencilOpsDict.Clear();
            m_stencilOpsDict = null;
        }

		public int Reference { get { return m_reference; } }
		public int ReadMask { get { return m_readMask; } }
		public int WriteMask { get { return m_writeMask; } }
		public int ComparisonFunctionIdx { get { return m_comparisonFunctionIdx; } }
		public int ComparisonFunctionBackIdx { get { return m_comparisonFunctionBackIdx; } }
		public int PassStencilOpIdx { get { return m_passStencilOpIdx; } }
		public int PassStencilOpBackIdx { get { return m_passStencilOpBackIdx; } }
		public int FailStencilOpIdx { get { return m_failStencilOpIdx; } }
		public int FailStencilOpBackIdx { get { return m_failStencilOpBackIdx; } }
		public int ZFailStencilOpIdx { get { return m_zFailStencilOpIdx; } }
		public int ZFailStencilOpBackIdx { get { return m_zFailStencilOpBackIdx; } }

	}
}
