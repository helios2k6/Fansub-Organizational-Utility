using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileNameParser
{
	/// <summary>
	/// Represents a media file that was produced by a Fansub group
	/// </summary>
	public class FansubFile : IEquatable<FansubFile>
	{
		/// <summary>
		/// The name of the fansub group
		/// </summary>
		public string FansubGroup { get; private set; }
		/// <summary>
		/// The name of the anime series
		/// </summary>
		public string SeriesName { get; private set; }
		/// <summary>
		/// The episode number
		/// </summary>
		public int EpisodeNumber { get; private set; }
		/// <summary>
		/// The file extension of the media file
		/// </summary>
		public string Extension { get; private set; }

		/// <summary>
		/// Constructs a new immutable FansubFile object. You almost certainly won't be constructing these yourself. 
		/// <seealso cref="FansubFileParsers"/>
		/// </summary>
		/// <param name="fansubGroup">The fansub group name</param>
		/// <param name="seriesName">The anime series name</param>
		/// <param name="episodeNumber">
		/// The episode number. If this isn't applicable, then <see cref="int.MinValue"/> should be used
		/// </param>
		/// <param name="extension">The file extension</param>
		public FansubFile(string fansubGroup, string seriesName, int episodeNumber, string extension)
		{
			FansubGroup = fansubGroup;
			SeriesName = seriesName;
			EpisodeNumber = episodeNumber;
			Extension = extension;
		}

		/// <summary>
		/// Makes a deep opy this <see cref="FansubFile"/>.
		/// </summary>
		/// <returns>A fresh <see cref="FansubFile"/>.</returns>
		public FansubFile DeepCopy()
		{
			return new FansubFile(FansubGroup, SeriesName, EpisodeNumber, Extension);
		}

		/// <summary>
		/// Determines if an object is equal to this <see cref="FansubFile"/>.
		/// </summary>
		/// <param name="right">The other object.</param>
		/// <returns>True if they are equal. False otherwise.</returns>
		public override bool Equals(object right)
		{
			if (object.ReferenceEquals(right, null)) return false;

			if (object.ReferenceEquals(this, right)) return true;

			if (this.GetType() != right.GetType()) return false;

			return this.Equals(right as FansubFile);
		}

		/// <summary>
		/// Determines whether two FansubFiles are equal
		/// </summary>
		/// <param name="other">The other FansubFile</param>
		/// <returns>True if the files are equal. False otherwise</returns>
		public bool Equals(FansubFile other)
		{
			return FansubGroup.Equals(other.FansubGroup)
				&& SeriesName.Equals(other.SeriesName)
				&& EpisodeNumber == other.EpisodeNumber
				&& Extension.Equals(other.Extension);
		}

		/// <summary>
		/// Get the hash code of this file
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return FansubGroup.GetHashCode() ^ SeriesName.GetHashCode() ^ EpisodeNumber ^ Extension.GetHashCode();
		}
	}
}
