/*
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using FFTPatcher.Datatypes;

namespace FFTPatcher
{
    public static class PspIso
    {
		#region�Instance�Variables�(6)�

        private static readonly long[] bootBinLocations = { 0x10000, 0x0FED8000 };
        private static byte[] buffer = new byte[1024];
        private const int bufferSize = 1024;
        private static byte[] euSizes = new byte[] { 0xA4, 0x84, 0x3A, 0x00, 0x00, 0x3A, 0x84, 0xA4 };
        public const long FFTPackLocation = 0x02C20000;
        private static byte[] jpSizes = new byte[] { 0xE4, 0xD9, 0x37, 0x00, 0x00, 0x37, 0xD9, 0xE4 };

		#endregion�Instance�Variables�

		#region�Public�Methods�(10)�

        /// <summary>
        /// Decrypts the ISO.
        /// </summary>
        /// <param name="filename">The filename of the ISO to decrypt.</param>
        public static void DecryptISO( string filename )
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream( filename, FileMode.Open );
                DecryptISO( stream );
            }
            catch( NotSupportedException )
            {
                throw;
            }
            finally
            {
                if( stream != null )
                {
                    stream.Flush();
                    stream.Close();
                    stream = null;
                }
            }
        }

        /// <summary>
        /// Decrypts the ISO.
        /// </summary>
        /// <param name="stream">The stream of the ISO to decrypt.</param>
        public static void DecryptISO( FileStream stream )
        {
            if( IsJP( stream ) )
            {
                CopyBytes( stream, 0xFA68000, 0x37D9E4, 0x10000, 0x37DB40 );
                stream.Seek( 0xC0A2, SeekOrigin.Begin );
                stream.Write( jpSizes, 0, 8 );
            }
            else if( IsUS( stream ) || IsEU( stream ) )
            {
                CopyBytes( stream, 0xFED8000, 0x3A84A4, 0x10000, 0x3A8600 );
                stream.Seek( 0xC0A2, SeekOrigin.Begin );
                stream.Write( euSizes, 0, 8 );
            }
            else
            {
                throw new NotSupportedException( "Unrecognized image." );
            }
        }

        /// <summary>
        /// Determines whether the specified stream is EU.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// 	<c>true</c> if the specified stream is EU; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEU( FileStream stream )
        {
            return CheckFile( stream, "ULES-00850", "ULES00850", new long[] { 0x8373, 0xE000 }, new long[] { 0x2C18128, 0x101EC3A8, 0x10232530 } );
        }

        /// <summary>
        /// Determines whether the specified stream is JP.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// 	<c>true</c> if the specified stream is JP; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsJP( FileStream stream )
        {
            return CheckFile( stream, "ULJM-05194", "ULJM05194", new long[] { 0x8373, 0xE000 }, new long[] { 0x2BF0128, 0xFD619FC, 0xFD97A5C } );
        }

        /// <summary>
        /// Determines whether the specified stream is US.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// 	<c>true</c> if the specified stream is US; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUS( FileStream stream )
        {
            return CheckFile( stream, "ULUS-10297", "ULUS10297", new long[] { 0x8373, 0xE000 }, new long[] { 0x2C18128, 0x101EC3A8, 0x10232530 } );
        }

        public static void PatchISO( BackgroundWorker worker, DoWorkEventArgs args, IGeneratePatchList patches )
        {
            PatchISO( patches.FileName, worker, patches );
        }

        /// <summary>
        /// Patches the ISO.
        /// </summary>
        /// <param name="filename">The filename of the ISO to patch.</param>
        public static void PatchISO( string filename, BackgroundWorker worker, IGeneratePatchList patches )
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream( filename, FileMode.Open );
                PatchISO( stream, worker, patches );
            }
            catch( NotSupportedException )
            {
                throw;
            }
            finally
            {
                if( stream != null )
                {
                    stream.Flush();
                    stream.Close();
                    stream = null;
                }
            }
        }

        /// <summary>
        /// Patches the ISO.
        /// </summary>
        /// <param name="stream">The stream of the ISO to patch.</param>
        public static void PatchISO( FileStream stream, BackgroundWorker worker, IGeneratePatchList patchList )
        {
            if( IsJP( stream ) )
            {
                throw new NotSupportedException( "Unrecognized image." );
            }

            int numberOfTasks = patchList.PatchCount * 2;
            int tasksComplete = 0;

            List<PatchedByteArray> patches = new List<PatchedByteArray>();

            Action<string> sendProgress =
                delegate( string message )
                {
                    worker.ReportProgress( ++tasksComplete * 100 / numberOfTasks, message );
                };

            if( patchList.RegenECC )
            {
                DecryptISO( stream );
                sendProgress( "Decrypting EBOOT.BIN" );
            }

            const Context context = Context.US_PSP;
            if( patchList.Abilities ) 
            { 
                patches.AddRange( FFTPatch.Abilities.GetPatches( context ) );
                sendProgress( "Getting Abilities patches" );
                sendProgress( "Getting Abilities patches" );
            }

            if( patchList.AbilityEffects )
            {
                patches.AddRange( FFTPatch.Abilities.AllEffects.GetPatches( context ) );
                sendProgress( "Getting Ability Effects patches" );
                sendProgress( "Getting Ability Effects patches" );
            }

            if( patchList.ActionMenus )
            {
                patches.AddRange( FFTPatch.ActionMenus.GetPatches( context ) );
                sendProgress( "Getting Action Menus patches" );
                sendProgress( "Getting Action Menus patches" );
            }

            if( patchList.ENTD.Exists( b => b == true ) )
            {
                IList<PatchedByteArray> entdPatches = FFTPatch.ENTDs.GetPatches( context );
                for( int i = 0; i < 5; i++ )
                {
                    if( patchList.ENTD[i] )
                    {
                        patches.Add( entdPatches[i] );
                        sendProgress( string.Format( "Getting ENTD {0} patches", i ) );
                    }
                }
            }

            if( patchList.FONT )
            {
                patches.AddRange( FFTPatch.Font.GetPatches( context ) );
                sendProgress( "Getting Font patches" );
                sendProgress( "Getting Font patches" );
            }
            if( patchList.FontWidths )
            {
                patches.AddRange( FFTPatch.Font.GlyphWidths.GetPatches( context ) );
                sendProgress( "Getting Font width patches" );
                sendProgress( "Getting Font width patches" );
            }
            if( patchList.InflictStatus )
            {
                patches.AddRange( FFTPatch.InflictStatuses.GetPatches( context ) );
                sendProgress( "Getting Inflict Status patches" );
                sendProgress( "Getting Inflict Status patches" );
            }
            if( patchList.ItemAttributes )
            {
                patches.AddRange( FFTPatch.ItemAttributes.GetPatches( context ) );
                sendProgress( "Getting Item Attributes patches" );
                sendProgress( "Getting Item Attributes patches" );
                sendProgress( "Getting Item Attributes patches" );
                sendProgress( "Getting Item Attributes patches" );
            }
            if( patchList.Items )
            {
                patches.AddRange( FFTPatch.Items.GetPatches( context ) );
                sendProgress( "Getting Item patches" );
                sendProgress( "Getting Item patches" );
                sendProgress( "Getting Item patches" );
                sendProgress( "Getting Item patches" );
            }
            if( patchList.JobLevels )
            {
                patches.AddRange( FFTPatch.JobLevels.GetPatches( context ) );
                sendProgress( "Getting Job Levels patches" );
                sendProgress( "Getting Job Levels patches" );
            }
            if( patchList.Jobs )
            {
                patches.AddRange( FFTPatch.Jobs.GetPatches( context ) );
                sendProgress( "Getting Jobs patches" );
                sendProgress( "Getting Jobs patches" );
            }
            if( patchList.MonsterSkills )
            {
                patches.AddRange( FFTPatch.MonsterSkills.GetPatches( context ) );
                sendProgress( "Getting Monster Skills patches" );
                sendProgress( "Getting Monster Skills patches" );
            }
            if( patchList.MoveFindItems )
            {
                patches.AddRange( FFTPatch.MoveFind.GetPatches( context ) );
                sendProgress( "Getting Move/Find Items patches" );
                sendProgress( "Getting Move/Find Items patches" );
            }
            foreach( PatchedByteArray patch in patchList.OtherPatches )
            {
                patches.Add( patch );
                sendProgress( "Getting other patches" );
            }
            if( patchList.Poach )
            {
                patches.AddRange( FFTPatch.PoachProbabilities.GetPatches( context ) );
                sendProgress( "Getting Poach Probabilities patches" );
                sendProgress( "Getting Poach Probabilities patches" );
            }
            if( patchList.Skillsets )
            {
                patches.AddRange( FFTPatch.SkillSets.GetPatches( context ) );
                sendProgress( "Getting Skillsets patches" );
                sendProgress( "Getting Skillsets patches" );
            }
            if( patchList.StatusAttributes )
            {
                patches.AddRange( FFTPatch.StatusAttributes.GetPatches( context ) );
                sendProgress( "Getting Status attributes patches" );
                sendProgress( "Getting Status attributes patches" );
            }

            foreach( PatchedByteArray patch in patches )
            {
                ApplyPatch( stream, patch );
                sendProgress( "Patching ISO" );
            }
        }

        /// <summary>
        /// Updates the BOOT.BIN file in a War of the Lions ISO image.
        /// </summary>
        /// <param name="stream">The stream that represents a War of the Lions ISO image.</param>
        /// <param name="location">The location in BOOT.BIN to update.</param>
        /// <param name="bytes">The bytes to update BOOT.BIN with.</param>
        public static void UpdateBootBin( FileStream stream, long location, byte[] bytes )
        {
            DecryptISO( stream );

            foreach( long loc in bootBinLocations )
            {
                stream.WriteArrayToPosition( bytes, loc + location );
            }
        }

        /// <summary>
        /// Updates the specified fftpack.bin file with new data.
        /// </summary>
        /// <param name="stream">The stream that represents a War of the Lions ISO image.</param>
        /// <param name="index">The index of the file in fftpack.bin to update.</param>
        /// <param name="bytes">The bytes to update the file with.</param>
        public static void UpdateFFTPack( FileStream stream, int index, byte[] bytes )
        {
            FFTPack.PatchFile( stream, index, 0, bytes );
        }

		#endregion�Public�Methods�

		#region�Private�Methods�(3)�

        private static void ApplyPatch( FileStream stream, PatchedByteArray patch )
        {
            if( patch.SectorEnum != null )
            {
                if( patch.SectorEnum.GetType() == typeof( PspIso.Sectors ) )
                {
                    stream.WriteArrayToPosition( patch.Bytes, (int)((PspIso.Sectors)patch.SectorEnum) * 2048 + patch.Offset );
                }
                else if( patch.SectorEnum.GetType() == typeof( FFTPack.Files ) )
                {
                    FFTPack.PatchFile( stream, (int)((FFTPack.Files)patch.SectorEnum), (int)patch.Offset, patch.Bytes );
                }
                else
                {
                    throw new ArgumentException( "invalid type" );
                }
            }
        }

        private static bool CheckFile( FileStream stream, string str1, string str2, long[] loc1, long[] loc2 )
        {
            byte[] str1bytes = str1.ToByteArray();
            foreach( long l in loc1 )
            {
                stream.Seek( l, SeekOrigin.Begin );
                stream.Read( buffer, 0, str1.Length );
                for( int i = 0; i < str1bytes.Length; i++ )
                {
                    if( buffer[i] != str1bytes[i] )
                        return false;
                }
            }

            byte[] str2bytes = str2.ToByteArray();
            foreach( long l in loc2 )
            {
                stream.Seek( l, SeekOrigin.Begin );
                stream.Read( buffer, 0, str2.Length );
                for( int i = 0; i < str2bytes.Length; i++ )
                {
                    if( buffer[i] != str2bytes[i] )
                        return false;
                }
            }

            return true;
        }

        private static void CopyBytes( FileStream stream, long src, long srcSize, long dest, long destOldSize )
        {
            long bytesRead = 0;
            while( (bytesRead + bufferSize) < srcSize )
            {
                stream.Seek( src + bytesRead, SeekOrigin.Begin );
                stream.Read( buffer, 0, bufferSize );
                stream.Seek( dest + bytesRead, SeekOrigin.Begin );
                stream.Write( buffer, 0, bufferSize );
                bytesRead += bufferSize;
            }

            stream.Seek( src + bytesRead, SeekOrigin.Begin );
            stream.Read( buffer, 0, (int)(srcSize - bytesRead) );
            stream.Seek( dest + bytesRead, SeekOrigin.Begin );
            stream.Write( buffer, 0, (int)(srcSize - bytesRead) );

            if( destOldSize > srcSize )
            {
                buffer = new byte[bufferSize];
                stream.Seek( dest + srcSize, SeekOrigin.Begin );
                stream.Write( buffer, 0, (int)(destOldSize - srcSize) );
            }
        }

		#endregion�Private�Methods�

        public enum Sectors
        {
            PSP_GAME_ICON0_PNG = 22560,
            PSP_GAME_PARAM_SFO = 22576,
            PSP_GAME_PIC0_PNG = 22416,
            PSP_GAME_PIC1_PNG = 22432,
            PSP_GAME_SYSDIR_BOOT_BIN = 130480,
            PSP_GAME_SYSDIR_EBOOT_BIN = 32,
            PSP_GAME_SYSDIR_UPDATE_DATA_BIN = 6032,
            PSP_GAME_SYSDIR_UPDATE_EBOOT_BIN = 1936,
            PSP_GAME_SYSDIR_UPDATE_PARAM_SFO = 1920,
            PSP_GAME_USRDIR_fftpack_bin = 22592,
            PSP_GAME_USRDIR_movie_001_HolyStone_pmf = 132368,
            PSP_GAME_USRDIR_movie_002_Opening_pmf = 190832,
            PSP_GAME_USRDIR_movie_003_Abduction_pmf = 198112,
            PSP_GAME_USRDIR_movie_004_Kusabue_pmf = 135360,
            PSP_GAME_USRDIR_movie_005_Get_away_pmf = 140288,
            PSP_GAME_USRDIR_movie_006_Reassume_Dilita_pmf = 144352,
            PSP_GAME_USRDIR_movie_007_Dilita_Advice_pmf = 150224,
            PSP_GAME_USRDIR_movie_008_Ovelia_and_Dilita_pmf = 156000,
            PSP_GAME_USRDIR_movie_009_Dilita_Musing_pmf = 166192,
            PSP_GAME_USRDIR_movie_010_Ending_pmf = 179264,
            PSP_GAME_USRDIR_movie_011_Russo_pmf = 183360,
            PSP_GAME_USRDIR_movie_012_Valuhurea_pmf = 186304,
            PSP_GAME_USRDIR_movie_013_StaffRoll_pmf = 202128,
            UMD_DATA_BIN = 28,
        }
public static class Files
        {


            public static class PSP_GAME
            {
                public const Sectors ICON0_PNG = Sectors.PSP_GAME_ICON0_PNG;
                public const Sectors PARAM_SFO = Sectors.PSP_GAME_PARAM_SFO;
                public const Sectors PIC0_PNG = Sectors.PSP_GAME_PIC0_PNG;
                public const Sectors PIC1_PNG = Sectors.PSP_GAME_PIC1_PNG;



                public static class SYSDIR
                {
                    public const Sectors BOOT_BIN = Sectors.PSP_GAME_SYSDIR_BOOT_BIN;
                    public const Sectors EBOOT_BIN = Sectors.PSP_GAME_SYSDIR_EBOOT_BIN;



                    public static class UPDATE
                    {
                        public const Sectors DATA_BIN = Sectors.PSP_GAME_SYSDIR_UPDATE_DATA_BIN;
                        public const Sectors EBOOT_BIN = Sectors.PSP_GAME_SYSDIR_UPDATE_EBOOT_BIN;
                        public const Sectors PARAM_SFO = Sectors.PSP_GAME_SYSDIR_UPDATE_PARAM_SFO;
                    }
                }
                public static class USRDIR
                {
                    public const Sectors fftpack_bin = Sectors.PSP_GAME_USRDIR_fftpack_bin;


                    public static class movie
                    {
                        public const Sectors _001_HolyStone_pmf = Sectors.PSP_GAME_USRDIR_movie_001_HolyStone_pmf;
                        public const Sectors _002_Opening_pmf = Sectors.PSP_GAME_USRDIR_movie_002_Opening_pmf;
                        public const Sectors _003_Abduction_pmf = Sectors.PSP_GAME_USRDIR_movie_003_Abduction_pmf;
                        public const Sectors _004_Kusabue_pmf = Sectors.PSP_GAME_USRDIR_movie_004_Kusabue_pmf;
                        public const Sectors _005_Get_away_pmf = Sectors.PSP_GAME_USRDIR_movie_005_Get_away_pmf;
                        public const Sectors _006_Reassume_Dilita_pmf = Sectors.PSP_GAME_USRDIR_movie_006_Reassume_Dilita_pmf;
                        public const Sectors _007_Dilita_Advice_pmf = Sectors.PSP_GAME_USRDIR_movie_007_Dilita_Advice_pmf;
                        public const Sectors _008_Ovelia_and_Dilita_pmf = Sectors.PSP_GAME_USRDIR_movie_008_Ovelia_and_Dilita_pmf;
                        public const Sectors _009_Dilita_Musing_pmf = Sectors.PSP_GAME_USRDIR_movie_009_Dilita_Musing_pmf;
                        public const Sectors _010_Ending_pmf = Sectors.PSP_GAME_USRDIR_movie_010_Ending_pmf;
                        public const Sectors _011_Russo_pmf = Sectors.PSP_GAME_USRDIR_movie_011_Russo_pmf;
                        public const Sectors _012_Valuhurea_pmf = Sectors.PSP_GAME_USRDIR_movie_012_Valuhurea_pmf;
                        public const Sectors _013_StaffRoll_pmf = Sectors.PSP_GAME_USRDIR_movie_013_StaffRoll_pmf;
                    }
                }
            }            public const Sectors UMD_DATA_BIN = Sectors.UMD_DATA_BIN;
        }
    }
}