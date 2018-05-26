// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEditor;

namespace AmplifyShaderEditor
{
	public sealed class TemplatePostProcessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
		{
            if ( !TemplatesManager.Initialized )
            {
                TemplatesManager.Init();
            }

			bool refreshMenuItems = false;
			for ( int i = 0; i < importedAssets.Length; i++ )
			{
				if ( TemplateHelperFunctions.CheckIfTemplate( importedAssets[ i ] ) )
				{
					refreshMenuItems = true;
					string guid = AssetDatabase.AssetPathToGUID( importedAssets[ i ] );
					TemplateDataParent templateData = TemplatesManager.GetTemplate( guid );
					if( templateData != null )
					{
						templateData.Reload();
					}
					else
					{
						string name = TemplatesManager.OfficialTemplates.ContainsKey( guid ) ? TemplatesManager.OfficialTemplates[ guid ] : string.Empty;
						TemplatesManager.AddTemplate( new TemplateMultiPass( name, guid ));
					}
				}
			}

			if ( deletedAssets.Length > 0 )
			{
				if ( deletedAssets[ 0 ].IndexOf( Constants.InvalidPostProcessDatapath ) < 0 )
				{
					for ( int i = 0; i < deletedAssets.Length; i++ )
					{
						string guid = AssetDatabase.AssetPathToGUID( deletedAssets[ i ] );
						TemplateDataParent templateData = TemplatesManager.GetTemplate( guid );
						if ( templateData != null )
						{
							// Close any window using that template
							int windowCount = IOUtils.AllOpenedWindows.Count;
							for ( int windowIdx = 0; windowIdx < windowCount; windowIdx++ )
							{
								TemplateMasterNode masterNode = IOUtils.AllOpenedWindows[ windowIdx ].CurrentGraph.CurrentMasterNode as TemplateMasterNode;
								if ( masterNode != null && masterNode.CurrentTemplate.GUID.Equals( templateData.GUID ) )
								{
									IOUtils.AllOpenedWindows[ windowIdx ].Close();
								}
							}

							TemplatesManager.RemoveTemplate( templateData );
							refreshMenuItems = true;
						}
					}
				}
			}

			for ( int i = 0; i < movedAssets.Length; i++ )
			{
				if ( TemplateHelperFunctions.CheckIfTemplate( movedAssets[ i ] ) )
				{
					refreshMenuItems = true;
					break;
				}
			}

			for ( int i = 0; i < movedFromAssetPaths.Length; i++ )
			{
				if ( TemplateHelperFunctions.CheckIfTemplate( movedFromAssetPaths[ i ] ) )
				{
					refreshMenuItems = true;
					break;
				}
			}
			if( refreshMenuItems )
			{
				refreshMenuItems = false;
				TemplatesManager.CreateTemplateMenuItems();

				int windowCount = IOUtils.AllOpenedWindows.Count;
				for( int windowIdx = 0; windowIdx < windowCount; windowIdx++ )
				{
					IOUtils.AllOpenedWindows[ windowIdx ].CurrentGraph.ForceCategoryRefresh();
				}
			}
		}
	}
}
