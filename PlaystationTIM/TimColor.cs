using System;
using System.Drawing;

namespace PlaystationTIM
{
    public struct TimColor
    {
        private const byte ALPHA_NONE = 0;
        private const byte ALPHA_SEMI = 128;
        private const byte ALPHA_FULL = 255;

        //(MSB)                                                                     (LSB)
        // 15 | 14 | 13 | 12 | 11 | 10 | 09 | 08 | 07 | 06 | 05 | 04 | 03 | 02 | 01 | 00 |
        // TP |           B            |           G            |           R            |
        //
        //STP - Transparency control bit
        //R - Red component (5 bits)
        //G - Green component(5 bits)
        //B - Blue component (5 bits)

        #region Reasons to use C++

        private const int RED_SIZE = 5;
        private const int RED_OFFSET = 0;
        private const ushort RED_MASK = ((1 << RED_SIZE) - 1) << RED_OFFSET;

        private const int GREEN_SIZE = 5;
        private const int GREEN_OFFSET = RED_OFFSET + RED_SIZE;
        private const ushort GREEN_MASK = ((1 << GREEN_SIZE) - 1) << GREEN_OFFSET;

        private const int BLUE_SIZE = 5;
        private const int BLUE_OFFSET = GREEN_OFFSET + GREEN_SIZE;
        private const ushort BLUE_MASK = ((1 << BLUE_SIZE) - 1) << BLUE_OFFSET;

        private const int STP_SIZE = 1;
        private const int STP_OFFSET = BLUE_OFFSET + BLUE_SIZE;
        private const ushort STP_MASK = ((1 << STP_SIZE) - 1) << STP_OFFSET;

        #endregion Reasons to use C++

        private ushort _value;

        public TimColor(ushort value)
        {
            _value = value;
        }

        #region Properties

        public ushort Value { get { return _value; } set { _value = value; } }

        public byte R
        {
            get { return Convert.ToByte((_value & RED_MASK) >> RED_OFFSET); }
            set { _value = (ushort)(_value & ~RED_MASK | (Convert.ToByte(value) << RED_OFFSET) & RED_MASK); }
        }

        public byte G
        {
            get { return Convert.ToByte((_value & GREEN_MASK) >> GREEN_OFFSET); }
            set { _value = (ushort)(_value & ~GREEN_MASK | (Convert.ToByte(value) << GREEN_OFFSET) & GREEN_MASK); }
        }

        public byte B
        {
            get { return Convert.ToByte((_value & BLUE_MASK) >> BLUE_OFFSET); }
            set { _value = (ushort)(_value & ~BLUE_MASK | (Convert.ToByte(value) << BLUE_OFFSET) & BLUE_MASK); }
        }

        public bool STP
        {
            get { return Convert.ToBoolean((_value & STP_MASK) >> STP_OFFSET); }
            set { _value = (ushort)(_value & ~STP_MASK | (Convert.ToByte(value) << STP_OFFSET) & STP_MASK); }
        }

        #endregion Properties

        public Color ToColor(TimTransparency transparency)
        {
            var alpha = byte.MaxValue;
            switch (transparency)
            {
                case TimTransparency.Black:
                    alpha = (this.R == 0 && this.G == 0 && this.B == 0 && STP == false) ? ALPHA_NONE : ALPHA_FULL;
                    break;

                case TimTransparency.Semi:
                    alpha = this.STP ? ALPHA_SEMI : ALPHA_FULL;
                    break;

                case TimTransparency.Full:
                    alpha = this.STP ? ALPHA_NONE : ALPHA_FULL;
                    break;
            }

            return Color.FromArgb(alpha, this.R * 8, this.G * 8, this.B * 8);
        }

        public static TimColor FromColor(Color color, bool alpha = false)
        {
            return new TimColor()
            {
                R = Convert.ToByte(color.R / 8),
                G = Convert.ToByte(color.G / 8),
                B = Convert.ToByte(color.B / 8),
                STP = alpha
            };
        }
    }
}