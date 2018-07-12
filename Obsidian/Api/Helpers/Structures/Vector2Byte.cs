﻿using System.IO;

namespace Obsidian.Api.Helpers.Structures
{
    /// <summary>
    /// Represents a Vector containing two bytes
    /// </summary>
    public class Vector2Byte
    {
        /// <summary>
        /// The X component
        /// </summary>
        public byte X { get; set; }
        /// <summary>
        /// The Y component
        /// </summary>
        public byte Y { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Vector2Byte"/> instance
        /// </summary>
        public Vector2Byte(byte x, byte y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Initializes a new <see cref="Vector2Byte"/> instance from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> to read from</param>
        public Vector2Byte(BinaryReader br)
        {
            this.X = br.ReadByte();
            this.Y = br.ReadByte();
        }

        /// <summary>
        /// Writes this <see cref="Vector2Byte"/> into a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="bw">The <see cref="BinaryWriter"/> to write to</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(this.X);
            bw.Write(this.Y);
        }
    }
}