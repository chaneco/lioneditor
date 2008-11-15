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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace FFTPatcher.SpriteEditor
{
    /// <summary>
    /// A FFT sprite.
    /// </summary>
    public class Sprite : PalettedImage
    {

        #region Properties (6)


        private bool Compressed { get; set; }

        private long OriginalSize { get; set; }

        /// <summary>
        /// Gets the pixels used to draw this sprite.
        /// </summary>
        public byte[] BytPixels { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this sprite is a SP2 file.
        /// </summary>
        public bool SP2 { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this sprite is a SPR file.
        /// </summary>
        public bool SPR { get; private set; }


        #endregion Properties

        #region Constructors (1)

        public Sprite( IList<byte> bytes )
        {
            OriginalSize = bytes.Count;
            Size = new Size( 256, 488 );
            if( bytes.Count == 0x8000 )
            {
                FromSP2( bytes );
            }
            else
            {
                FromSPR( bytes );
            }
        }

        #endregion Constructors

        #region Methods (11)


        private static Palette BuildGreyscalePalette()
        {
            Color[] colors = new Color[16];
            for( int i = 0; i < 16; i++ )
            {
                colors[i] = Color.FromArgb( (byte)(i << 4), (byte)(i << 4), (byte)(i << 4) );
            }

            return new Palette( colors );
        }

        private static byte[] BuildPixels( IList<byte> bytes, IList<byte> compressedBytes )
        {
            List<byte> result = new List<byte>( 36864 * 2 );
            foreach( byte b in bytes )
            {
                result.Add( b.GetLowerNibble() );
                result.Add( b.GetUpperNibble() );
            }

            if( compressedBytes.Count > 0 )
            {
                result.AddRange( Decompress( compressedBytes ) );
            }

            return result.ToArray();
        }

        private static byte[] CompressNibbles( IList<byte> bytes )
        {
            List<byte> result = new List<byte>( bytes.Count / 2 );
            for( int i = 0; i < bytes.Count; i += 2 )
            {
                if( (i + 1) < bytes.Count )
                {
                    result.Add( (byte)(((bytes[i] & 0x0F) << 4) + (bytes[i + 1] & 0x0F)) );
                }
                else
                {
                    result.Add( (byte)((bytes[i] & 0x0F) << 4) );
                }
            }
            return result.ToArray();
        }

        private static byte[] Decompress( IList<byte> bytes )
        {
            byte[] compressed = new byte[bytes.Count * 2];
            for( int i = 0; i < bytes.Count; i++ )
            {
                compressed[i * 2] = bytes[i].GetUpperNibble();
                compressed[i * 2 + 1] = bytes[i].GetLowerNibble();
            }

            List<byte> result = new List<byte>();
            int j = 0;
            while( j < compressed.Length )
            {
                byte b = compressed[j];
                if( compressed[j] != 0 )
                {
                    result.Add( compressed[j] );
                }
                else if( (j + 1) < compressed.Length )
                {
                    byte s = compressed[j + 1];
                    int l = s;
                    if( (s == 7) && ((j + 3) < compressed.Length) )
                    {
                        l = compressed[j + 2] + (compressed[j + 3] << 4);
                        j += 2;
                    }
                    else if( (s == 8) && ((j + 4) < compressed.Length) )
                    {
                        l = compressed[j + 2] + (compressed[j + 3] << 4) + (compressed[j + 4] << 8);
                        j += 3;
                    }
                    else if( (s == 0) && ((j + 2) < compressed.Length) )
                    {
                        l = compressed[j + 2];
                        j++;
                    }
                    else
                    {
                        l = s;
                    }

                    j++;
                    for( int k = 0; k < l; k++ )
                        result.Add( 0x00 );
                }

                j++;
            }

            j = 0;
            while( (j + 1) < result.Count )
            {
                byte k = result[j];
                result[j] = result[j + 1];
                result[j + 1] = k;
                j += 2;
            }

            return result.ToArray();
        }

        private void FromSP2( IList<byte> bytes )
        {
            SP2 = true;
            SPR = false;
            BytPixels = BuildPixels( bytes, new byte[0] );
            Compressed = false;
            Palettes = new Palette[16];
            for( int i = 0; i < 16; i++ )
                Palettes[i] = BuildGreyscalePalette();
        }

        private void FromSPR( IList<byte> bytes )
        {
            SPR = true;
            SP2 = false;
            Palettes = new Palette[16];
            for( int i = 0; i < 16; i++ )
            {
                Palettes[i] = new Palette( bytes.Sub( i * 32, (i + 1) * 32 - 1 ) );
            }

            if( bytes.Count < 0x9200 || ((bytes.Count > 0x9200) && (bytes[0x9200] == 0x00)) )
            {
                BytPixels = BuildPixels( bytes.Sub( 16 * 32 ), new byte[0] );
                Compressed = false;
            }
            else
            {
                BytPixels = BuildPixels( bytes.Sub( 16 * 32, 16 * 32 + 36864 - 1 ), bytes.Sub( 16 * 32 + 36864 ) );
                Compressed = true;
            }
        }

        private static int NumberOfZeroes( IList<byte> bytes )
        {
            for( int i = 0; i < bytes.Count; i++ )
            {
                if( bytes[i] != 0 )
                    return i;
            }

            return bytes.Count;
        }

        private static byte[] Recompress( IList<byte> bytes )
        {
            List<byte> realBytes = new List<byte>( bytes.Count );
            for( int i = 0; (i + 1) < bytes.Count; i += 2 )
            {
                realBytes.Add( bytes[i + 1] );
                realBytes.Add( bytes[i] );
            }

            List<byte> result = new List<byte>();
            int pos = 0;
            while( pos < realBytes.Count )
            {
                int z = NumberOfZeroes( realBytes.Sub( pos ) );
                z = Math.Min( z, 0xFFF );

                if( z == 0 )
                {
                    byte b = realBytes[pos];
                    result.Add( realBytes[pos] );
                    pos += 1;
                }
                else if( z < 16 )
                {
                    if( (z == 8) ||
                        (z == 7) )
                    {
                        result.Add( 0x00 );
                        result.Add( 0x00 );
                        result.Add( (byte)z );
                    }
                    else
                    {
                        result.Add( 0x00 );
                        result.Add( (byte)z );
                    }
                }
                else if( z < 256 )
                {
                    result.Add( 0x00 );
                    result.Add( 0x07 );
                    result.Add( ((byte)z).GetLowerNibble() );
                    result.Add( ((byte)z).GetUpperNibble() );
                }
                else if( z < 4096 )
                {
                    result.Add( 0x00 );
                    result.Add( 0x08 );
                    result.Add( ((byte)z).GetLowerNibble() );
                    result.Add( ((byte)z).GetUpperNibble() );
                    result.Add( (byte)((z & 0xF00) >> 8) );
                }

                pos += z;
            }

            return CompressNibbles( result );
        }

        /// <summary>
        /// Imports a bitmap and tries to convert it to a FFT sprite.
        /// </summary>
        public void ImportBitmap( Bitmap bmp, out bool foundBadPixels )
        {
            foundBadPixels = false;

            if( bmp.PixelFormat != PixelFormat.Format8bppIndexed )
            {
                throw new BadImageFormatException();
            }
            if( bmp.Width != 256 )
            {
                throw new BadImageFormatException();
            }

            Palettes = new Palette[16];
            for( int i = 0; i < 16; i++ )
            {
                Palettes[i] = new Palette( bmp.Palette.Entries.Sub( 16 * i, 16 * (i + 1) - 1 ) );
            }

            BitmapData bmd = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite, bmp.PixelFormat );
            for( int i = 0; (i < BytPixels.Length) && (i / 256 < bmp.Height); i++ )
            {
                BytPixels[i] = (byte)bmd.GetPixel( i % 256, i / 256 );
                if( BytPixels[i] >= 16 )
                {
                    foundBadPixels = true;
                }
            }

            bmp.UnlockBits( bmd );
        }

        public override void Import( Image file )
        {
            if( file is Bitmap )
            {
                bool bad;
                ImportBitmap( file as Bitmap, out bad );
            }
            else
            {
                throw new ArgumentException( "file must be Bitmap", "file" );
            }
        }


        /// <summary>
        /// Converts this sprite to an indexed bitmap.
        /// </summary>
        public unsafe Bitmap ToBitmap()
        {
            return ToBitmap( false );
        }

        public unsafe Bitmap ToBitmap( bool proper )
        {
            Bitmap bmp = new Bitmap( 256, 488, PixelFormat.Format8bppIndexed );
            ColorPalette palette = bmp.Palette;

            int k = 0;
            for( int i = 0; i < Palettes.Count; i++ )
            {
                for( int j = 0; j < Palettes[i].Colors.Length; j++, k++ )
                {
                    if( Palettes[i].Colors[j].ToArgb() == Color.Transparent.ToArgb() )
                    {
                        palette.Entries[k] = Color.Black;
                    }
                    else
                    {
                        palette.Entries[k] = Palettes[i].Colors[j];
                    }
                }
            }
            bmp.Palette = palette;

            BitmapData bmd = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite, bmp.PixelFormat );
            if( proper )
            {
                for( int i = 0; (i < this.BytPixels.Length) && (i / 256 < 256); i++ )
                {
                    bmd.SetPixel( i % 256, i / 256, BytPixels[i] );
                }
                for( int i = 288 * 256; (i < this.BytPixels.Length) && (i / 256 < 488); i++ )
                {
                    bmd.SetPixel( i % 256, i / 256 - 32, BytPixels[i] );
                }
                for( int i = 256 * 256; (i < this.BytPixels.Length) && (i / 256 < 288); i++ )
                {
                    bmd.SetPixel( i % 256, i / 256 + 200, BytPixels[i] );
                }
            }
            else
            {
                for( int i = 0; (i < this.BytPixels.Length) && (i / 256 < bmp.Height); i++ )
                {
                    bmd.SetPixel( i % 256, i / 256, BytPixels[i] );
                }
            }
            bmp.UnlockBits( bmd );

            return bmp;
        }

        public override Image Export()
        {
            return ToBitmap();
        }

        /// <summary>
        /// Converts this sprite to an array of bytes.
        /// </summary>
        public override byte[] ToByteArray()
        {
            List<byte> result = new List<byte>();
            if( SPR && !SP2 )
            {
                foreach( Palette p in Palettes )
                {
                    result.AddRange( p.ToByteArray() );
                }
            }

            for(
                int i = 0;
                (Compressed && (i < 36864) && (2 * i + 1 < BytPixels.Length)) ||
                (!Compressed && (2 * i + 1 < BytPixels.Length));
                i++ )
            {
                result.Add( (byte)((BytPixels[2 * i + 1] << 4) | (BytPixels[2 * i] & 0x0F)) );
            }

            if( BytPixels.Length > 2 * 36864 && Compressed )
            {
                result.AddRange( Recompress( BytPixels.Sub( 2 * 36864 ) ) );
            }

            if( result.Count < OriginalSize )
            {
                result.AddRange( new byte[OriginalSize - result.Count] );
            }

            return result.ToArray();
        }

        #endregion Methods

        public override void Draw( Graphics graphics, int paletteIndex )
        {
            graphics.DrawSprite( this, Palettes[paletteIndex], Palettes[(paletteIndex + 8) % 8 + 8], true );
        }
    }
}