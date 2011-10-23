using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.IO;

namespace Microsoft.LearningComponents
{
    class MrciCompression
    {
        byte[] compressedData;
        int inputLength;
        int position = 0;
        const int MAXOUTPERTOKEN = 513 + 3;     //maximum uncompressed bytes per compressed token
        const int MAXDISPBIG = 4095;                // Maximum size of big displacement
        const int MAXDISPSMALL = 64;                // Maximum size of small displacement
        const int MAXDISPMED = 256 + MAXDISPSMALL; // Maximum size of medium displacement
        int outputPosition = 0;
        byte[] output;

#region algorithm
// $Review...
// Implementation notes from developer with transcriptions:
// I don't know who 'developer is', but I (Richard Willis) converted
// to managed code from the original C++.
//
//   -- The MRCI2 alrorithm (implemented here) is based on LZ77.  It
//      compresses 3-byte (or greater) repeated sequences.
//
//   -- All bytes need to be read with least significant bit first. So 33
//      is read 10000100 instead of the normal 00100001.
//
//   -- The compressed stream is conceptually a series of bits, organized
//      into tokens.  A token may be:
//        1. 0xxxxxxx (8 bits): decompresses into literal byte xxxxxxx
//           (in the range 0x00 to 0x7F);
//        2. 11xxxxxxx (9 bits): decompresses into literal byte xxxxxxx + 128
//           (in the range 0x80 to 0xFF);
//        3. 100yyyyyy (9 bits) followed by a "byte count" (see below): 
//           decompresses to the previously-output substring located yyyyyy
//           bytes (yyyyyy is a 6-bit "small displacement") before the end of
//           the current output buffer;
//        4. 1010yyyyyyyy (12 bits) followed by a "byte count" (see below): 
//           decompresses to the previously-output substring located yyyyyyyy
//           bytes (yyyyyyyy is an 8-bit "medium displacement") before the end
//           of the current output buffer;
//        5. 1011yyyyyyyyyyyy (16 bits) followed by a "byte count" (see below): 
//           decompresses to the previously-output substring located
//           yyyyyyyyyyyy bytes (yyyyyyyyyyyy is a 12-bit "big displacement")
//           before the end of the current output buffer.
//        6. 1011111111111111 (16 bits) = MAXDISPBIG: this is a special token
//           which indicates the end of a sector (a 512-byte block).
//
//   -- The "byte count" portion of a token specifies the length of the
//      matched substring.  The byte count is represented by N 0 bits,
//      followed by a 1 bit, followed by N bits that (partially) indicate
//      the byte count.  Specifically:
//        -- 1: means byte count is 3 (since 3 is the minimum byte count)
//        -- 01z: means byte count is 4 + z (e.g. 010 = 4-byte match, 011 = 5)
//        -- 001zz: means byte count is 6 + zz (i.e. 6, 7, 8, or 9 byte match)
//        -- 0001zzz: means byte count is 10 + zzz (i.e. 10 to 17 byte match)
//        --   ...
//        -- 000000001zzzzzzzz: (i.e. 257 to 412 byte match)
//           $Review: is this the maximum?
//
//   -- Best possible theoretical compression: one literal (e.g. 0xxxxxxx)
//      followed by aa series of tokens of the form
//      11xxxxxxx00000000111111111 (26 bits), where xxxxxxx is anything;
//      this 26-bit token specifies a run of 513 bytes, yielding an effective
//      compression ratio of about 157.5 to 1
//
//      (Developer says: "I would expect a run length of 9 would be needed to get
//      a MAXMATCH of 512.  8 bits would only get you to 257 or so.")
//
//   -- If the byte count is greater than the displacement (e.g. byte count
//      is 7, displacement is 2 pointing to substring "AB") then the matched
//      substring is repeated as many times as necessary to complete
//      (resulting in "ABABABA" in the previous example).
#endregion algorithm

#region constructors
        /// <summary>Initializes a new instance of <see cref="Mrci"/>.</summary>
        /// <param name="compressed">The compressed data.</param>
        /// <param name="messageLength">The length of the output data. The LRM header tells us this so we
        /// can use it to optimise memory allocation.</param>
        public MrciCompression(byte[] compressed, int messageLength)
        {
            this.compressedData = compressed;
            inputLength = compressed.Length * 8;

            // allocate output buffer; allocate MAXOUTPERTOKEN more than we think
            // we need, so that we don't have to test for an out-of-buffer condition
            // on every function call
            output = new byte[messageLength + MAXOUTPERTOKEN];
        }
#endregion constructors

#region public methods
        /// <summary>Decompresses the data.</summary>
        /// <returns>The decompressed data.</returns>
        public byte[] Decompress()
        {
            while (position < inputLength)
            {
                if (ReadBit() == false)
                {
                    // Case 1: token starts with 0
                    Write(7);
                }
                else
                {
                    // Case 1: token starts with 0
                    if (ReadBit())
                    {
                        // Case 2: token starts with 11
                        Write(7, 128);
                    }
                    else
                    {
                        // token starts with 10
                        if (ReadBit())
                        {
                            // token starts with 101
                            if (ReadBit())
                            {
                                // token starts with 1011. Sector end is handled in HandleDisplacement.
                                HandleDisplacement(12, MAXDISPMED);
                            }
                            else
                            {
                                // Case 4: token starts with 1010
                                HandleDisplacement(8, MAXDISPSMALL);
                            }
                        }
                        else
                        {
                            // Case 3: token starts with 100
                            HandleDisplacement(6, 0);
                        }
                    }
                }
            }

            return output;
        }
#endregion public methods

#region private methods
        /// <summary>Tests if a bytes has the given bit set.</summary>
        /// <param name="byteValue">The byte value.</param>
        /// <param name="bit">The bit to check.</param>
        /// <returns>True if the bit is set.</returns>
        bool IsBitSet(byte byteValue, int bit)
        {
            if ((byteValue & bit) == bit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>Reads a number of bits from the input.</summary>
        /// <param name="length">The number of bits to read.</param>
        /// <returns>Converts to an int value (least significant bit first) to return.</returns>
        int Read(int length)
        {
            if (position + length > inputLength)
            {
                return 0;
            }

            int value = 0;

            for (int i = 0; i < length; i++)
            {
                if (ReadBit())
                {
                    value += (int)Math.Pow(2,i);
                }
            }

            return value;
        }

        /// <summary>Writes a number of bits from the input directly to the output.</summary>
        /// <param name="length">The number of bits to write.</param>
        void Write(int length)
        {
            Write(length, 0);
        }

        /// <summary>Writes a number of bits from the input to the output after adding an additional value.</summary>
        /// <param name="length">The number of bits to write.</param>
        /// <param name="additionalValue">The additional value to add.</param>
        void Write(int length, byte additionalValue)
        {
            byte toWrite = (byte)((byte)Read(length) + additionalValue);
            output[outputPosition] = toWrite;
            outputPosition++;
        }

        /// <summary>Reads a bit from the input and advances the position.</summary>
        /// <returns>The bit value.</returns>
        bool ReadBit()
        {
            int bitNumber = position % 8;
            int byteNumber = (position - bitNumber) / 8;

            byte b = compressedData[byteNumber];

            position++;
            bool value;

            switch (bitNumber)
            {
                case 0:
                    value = IsBitSet(b , 1);
                    break;
                case 1:
                    value = IsBitSet(b , 2);
                    break;
                case 2:
                    value = IsBitSet(b , 4);
                    break;
                case 3:
                    value = IsBitSet(b , 8);
                    break;
                case 4:
                    value = IsBitSet(b , 16);
                    break;
                case 5:
                    value = IsBitSet(b , 32);
                    break;
                case 6:
                    value = IsBitSet(b , 64);
                    break;
                case 7:
                    value = IsBitSet(b , 128);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return value;

        }

        /// <summary>Calculates the byte count of the displacement.</summary>
        /// <returns>The number of bytes to add to the output.</returns>
        int CalculateByteCount()
        {
            bool value = false;
            int numberOfZeros = -1;

            do 
            {
                value = ReadBit();
                numberOfZeros++;
            } while (value == false);

            if (numberOfZeros > 8)
            {
                throw new InvalidOperationException();
            }

            int count = 2 + (int)Math.Pow(2,numberOfZeros);;
            count += Read(numberOfZeros);
            return count;
        }

        /// <summary>Writes the displacement to the output.</summary>
        /// <param name="displacement">The position of the displacement.</param>
        /// <param name="count">The number of bytes to copy.</param>
        void WriteDisplacement(int displacement, int count)
        {
            if (displacement >= count)
            {
                CopyOutput(displacement, count);
            }
            else
            {
                CopyOutput(displacement, displacement);
                WriteDisplacement(displacement, count - displacement);
            }
        }

        /// <summary>Copies part of the output to itself.</summary>
        /// <param name="displacement">The position to start copying.</param>
        /// <param name="count">The number of bytes to copy.</param>
        void CopyOutput(int displacement, int count)
        {
            Array.Copy(output,outputPosition - displacement, output, outputPosition, count);
            outputPosition += count;
        }

        /// <summary>Processes a displacement.</summary>
        /// <param name="numberOfBits">The number of bits to use to determine the displacement.</param>
        /// <param name="weighting">An additional value to add to the displacement.</param>
        void HandleDisplacement(int numberOfBits, int weighting)
        {
            int displacement = Read(numberOfBits);
            if (displacement == MAXDISPBIG)
            {
                // Case 6: MAXDISPBIG End of sector
            }
            else
            {
                displacement = displacement + weighting;
                int count = CalculateByteCount();
                WriteDisplacement(displacement, count);
            }
        }
#endregion private methods

    }
}

