using System;

namespace PlaystationTIM
{
    public struct TimFlag
    {
        //(MSB)                                                                                      (LSB)
        // 32 | ... | 16 | 15 | 14 | 13 | 12 | 11 | 10 | 09 | 08 | 07 | 06 | 05 | 04 | 03 | 02 | 01 | 00 |
        //                                    Reserved                               | CF |     PMODE    |

        #region Reasons to use C++

        private const int PMODE_SIZE = 3;
        private const int PMODE_OFFSET = 0;
        private const uint PMODE_MASK = ((1u << PMODE_SIZE) - 1u) << PMODE_OFFSET;

        private const int CF_SIZE = 1;
        private const int CF_OFFSET = PMODE_OFFSET + PMODE_SIZE;
        private const uint CF_MASK = ((1u << CF_SIZE) - 1u) << CF_OFFSET;

        private const int RESERVED_SIZE = 28;
        private const int RESERVED_OFFSET = CF_OFFSET + CF_SIZE;
        private const uint RESERVED_MASK = ((1u << RESERVED_SIZE) - 1u) << RESERVED_OFFSET;

        #endregion Reasons to use C++

        private uint _value;

        public TimFlag(uint value)
        {
            _value = value;
        }

        #region Properties

        public uint Value { get { return _value; } set { _value = value; } }

        public TimPixelMode PixelMode
        {
            get { return (TimPixelMode)Convert.ToByte((_value & PMODE_MASK) >> PMODE_OFFSET); }
            set { _value = (ushort)(_value & ~PMODE_MASK | (Convert.ToByte(value) << PMODE_OFFSET) & PMODE_MASK); }
        }

        public bool HasClut
        {
            get { return Convert.ToBoolean((_value & CF_MASK) >> CF_OFFSET); }
            set { _value = (ushort)(_value & ~CF_MASK | (Convert.ToByte(value) << CF_OFFSET) & CF_MASK); }
        }

        public uint Reserved
        {
            get { return Convert.ToUInt32((_value & RESERVED_MASK) >> RESERVED_OFFSET); }
            set { _value = (uint)(_value & ~RESERVED_MASK | (Convert.ToUInt32(value) << RESERVED_OFFSET) & RESERVED_MASK); }
        }

        #endregion Properties
    }
}