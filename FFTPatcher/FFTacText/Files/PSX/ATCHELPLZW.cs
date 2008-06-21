﻿/*
    Copyright 2007, Joe Davidson <joedavidson@gmail.com>

    This file is part of FFTPatcher.

    FFTPatcher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FFTPatcher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FFTPatcher.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

namespace FFTPatcher.TextEditor.Files.PSX
{
    public class ATCHELPLZW : BasePSXSectionedFile
    {

        #region Static Fields (3)

        //private static string[][] entryNames;
        private static readonly IList<IList<string>> entryNames;
        private static Dictionary<string, long> locations;
        private static string[] sectionNames = new string[21] {
            "", "", "", "", "", "", 
            "", "", "", "", "", "Help", 
            "Job descriptions", "Item descriptions", "", "Ability descriptions", "", "",
            "", "Skillset descriptions", "" };

        #endregion Static Fields

        #region Fields (1)

        private const string filename = "ATCHELP.LZW";

        #endregion Fields

        #region Properties (6)


        /// <summary>
        /// Gets the number of sections.
        /// </summary>
        /// <value>The number of sections.</value>
        protected override int NumberOfSections { get { return 21; } }

        /// <summary>
        /// Gets a collection of lists of strings, each string being a description of an entry in this file.
        /// </summary>
        /// <value></value>
        public override IList<IList<string>> EntryNames { get { return entryNames; } }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value></value>
        public override string Filename { get { return filename; } }

        /// <summary>
        /// Gets the filenames and locations for this file.
        /// </summary>
        /// <value></value>
        public override IDictionary<string, long> Locations
        {
            get
            {
                if( locations == null )
                {
                    locations = new Dictionary<string, long>();
                    locations.Add( "EVENT/ATCHELP.LZW", 0x00 );
                }

                return locations;
            }
        }

        /// <summary>
        /// Gets the maximum length of this file as a byte array.
        /// </summary>
        /// <value></value>
        public override int MaxLength { get { return 0x0160D5; } }

        /// <summary>
        /// Gets a collection of strings with a description of each section in this file.
        /// </summary>
        /// <value></value>
        public override IList<string> SectionNames { get { return sectionNames; } }


        #endregion Properties

        #region Constructors (3)

        static ATCHELPLZW()
        {
            entryNames = FFTPatcher.TextEditor.Files.PSX.EntryNames.GetEntryNames( typeof( ATCHELPLZW ) );

            //entryNames = new string[21][];
            //int[] sectionLengths = new int[] { 
            //    1, 1, 1, 1, 1,
            //    1, 1, 1, 1, 1,
            //    1, 40, 160, 256, 1, 
            //    512, 1, 1, 1, 188,
            //    1 };

            //for( int i = 0; i < entryNames.Length; i++ )
            //{
            //    entryNames[i] = new string[sectionLengths[i]];
            //}
            //entryNames[11] = new string[40] {
            //    "Unit #", "Level", "HP", "MP", "CT", "AT", "Exp", "Name",
            //    "Brave", "Faith", "", "Move", "Jump", "", "", "",
            //    "Speed", "ATK", "Weapon ATK", "", "Eva%", "SEv%", "AEv%", "Phys land effect",
            //    "Magic land effect", "Estimated", "Hit rate", "Aries", "Taurus", "Gemini", "Cancer", "Leo", 
            //    "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces", "Serpentarius" };
            //IList<string> temp = new List<string>( FFTPatcher.Datatypes.AllJobs.PSXNames.Sub( 0, 154 ) );
            //temp.AddRange( new string[sectionLengths[12] - temp.Count] );
            //entryNames[12] = temp.ToArray();
            //entryNames[13] = FFTPatcher.Datatypes.Item.PSXNames.ToArray();
            //temp = new List<string>( new string[265] );
            //temp.AddRange( FFTPatcher.Datatypes.AllAbilities.PSXNames.Sub( 265 ) );
            //entryNames[15] = temp.ToArray();
            //temp = new List<string>( FFTPatcher.Datatypes.SkillSet.PSXNames.Sub( 0, 0xAF ) );
            //temp.AddRange( new string[12] );
            //entryNames[19] = temp.ToArray();
        }

        private ATCHELPLZW()
        {
        }

        public ATCHELPLZW( IList<byte> bytes )
            : base( bytes )
        {
        }

        #endregion Constructors

    }
}
