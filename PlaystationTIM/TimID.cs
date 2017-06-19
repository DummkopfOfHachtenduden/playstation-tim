using System;

namespace PlaystationTIM
{
    public struct TimID
    {
        //(MSB)                                                                                      (LSB)
        // 32 | ... | 16 | 15 | 14 | 13 | 12 | 11 | 10 | 09 | 08 | 07 | 06 | 05 | 04 | 03 | 02 | 01 | 00 |
        //   Reserved    |              Version No.              |                ID                     |

        #region Reasons to use C++

        private const int ID_SIZE = 8;
        private const int ID_OFFSET = 0;
        private const uint ID_MASK = ((1u << ID_SIZE) - 1u) << ID_OFFSET;

        private const int VERSION_SIZE = 8;
        private const int VERSION_OFFSET = ID_OFFSET + ID_SIZE;
        private const uint VERSION_MASK = ((1u << VERSION_SIZE) - 1u) << VERSION_OFFSET;

        private const int RESERVED_SIZE = 16;
        private const int RESERVED_OFFSET = VERSION_OFFSET + VERSION_SIZE;
        private const uint RESERVED_MASK = ((1u << RESERVED_SIZE) - 1u) << RESERVED_OFFSET;

        #endregion Reasons to use C++

        private uint _value;

        public TimID(uint value)
        {
            _value = value;
        }

        #region Properties

        public uint Value { get { return _value; } set { _value = value; } }

        public byte ID
        {
            get { return Convert.ToByte((_value & ID_MASK) >> ID_OFFSET); }
            set { _value = (ushort)(_value & ~ID_MASK | (Convert.ToByte(value) << ID_OFFSET) & ID_MASK); }
        }

        public byte Version
        {
            get { return Convert.ToByte((_value & VERSION_MASK) >> VERSION_OFFSET); }
            set { _value = (ushort)(_value & ~VERSION_MASK | (Convert.ToByte(value) << VERSION_OFFSET) & VERSION_MASK); }
        }

        public ushort Reserved
        {
            get { return Convert.ToUInt16((_value & RESERVED_MASK) >> RESERVED_OFFSET); }
            set { _value = (ushort)(_value & ~RESERVED_MASK | (Convert.ToUInt16(value) << RESERVED_OFFSET) & RESERVED_MASK); }
        }

        #endregion Properties
    }
}