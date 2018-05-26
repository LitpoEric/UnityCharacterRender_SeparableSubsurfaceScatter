// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;


namespace AmplifyShaderEditor
{
	[Serializable]
	public class VersionInfo
	{
		public const byte Major = 1;
		public const byte Minor = 5;
		public const byte Release = 3;
		public const byte Revision = 1;
		
		private static string StageSuffix = "_dev0"+Revision;

		public static string StaticToString()
		{
			return string.Format( "{0}.{1}.{2}", Major, Minor, Release ) + StageSuffix;
		}

		public override string ToString()
		{
			return string.Format( "{0}.{1}.{2}", m_major, m_minor, m_release ) + StageSuffix;
		}

		public int Number { get { return m_major * 10000 + m_minor * 1000 + m_release * 100 + m_revision; } }
		
		[SerializeField] private int m_major;
		[SerializeField] private int m_minor;
		[SerializeField] private int m_release;
		[SerializeField] private int m_revision;

		[SerializeField] private int m_fullNumber;

		[SerializeField] private string m_fullLabel;

		public VersionInfo()
		{
			m_major = Major;
			m_minor = Minor;
			m_release = Release;
			m_revision = Revision;
			m_fullNumber = Number;
			m_fullLabel = "Version=" + m_fullNumber;
		}

		public VersionInfo( byte major, byte minor, byte release , byte revision )
		{
			m_major = major;
			m_minor = minor;
			m_release = release;
			m_revision = revision;
			m_fullNumber = Number;
			m_fullLabel = "Version=" + m_fullNumber;
		}

		public int FullNumber { get { return m_fullNumber; } }
		public string FullLabel { get { return m_fullLabel; } }

		public static VersionInfo Current()
		{
			return new VersionInfo( Major, Minor, Release ,Revision);
		}

		public static bool Matches( VersionInfo version )
		{
			return ( Major == version.m_major ) && ( Minor == version.m_minor ) && ( Release == version.m_release ) && (Revision == version.m_revision);
		}
	}
}
