#define UNSAFE

using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace PlaystationTIM
{
    public class Tim : IDisposable
    {
        public int Width
        {
            get
            {
                switch (_flag.PixelMode)
                {
                    case TimPixelMode.CLUT4:
                        return _pixelDataWidth * 4;

                    case TimPixelMode.CLUT8:
                        return _pixelDataWidth * 2;

                    case TimPixelMode.Direct15:
                        return _pixelDataWidth;

                    case TimPixelMode.Direct24:
                        return _pixelDataWidth / 2;

                    case TimPixelMode.Mixed:
                        throw new NotSupportedException("Mixed mode is not supported.");

                    default:
                        return 1;
                }
            }
        }

        public int Height => _pixelDataHeight;

        public TimID ID => _id;
        public TimFlag Flag => _flag;

        public int ClutIndex { get; set; }
        public TimTransparency Transparency { get; set; }

        public TimColor[][] CLUT => _clutData;

        private TimID _id;
        private TimFlag _flag;

        private ushort _clutDataX;
        private ushort _clutDataY;
        private ushort _clutDataWidth;
        private ushort _clutDataHeight;
        private TimColor[][] _clutData;

        private ushort _pixelDataX;
        private ushort _pixelDataY;
        private ushort _pixelDataWidth;
        private ushort _pixelDataHeight;
        private ushort[][] _pixelData;

        public Tim(byte[] data)
        {
            using (var ms = new MemoryStream(data))
                this.Load(ms);
        }

        public Tim(Stream stream)
        {
            this.Load(stream);
        }

        private void Load(Stream steam)
        {
            using (var reader = new BinaryReader(steam, Encoding.ASCII, true))
            {
                _id = new TimID(reader.ReadUInt32());
                _flag = new TimFlag(reader.ReadUInt32());

                if (_flag.HasClut)
                {
                    if (_flag.PixelMode == TimPixelMode.Direct15 ||
                        _flag.PixelMode == TimPixelMode.Direct24)
                        throw new NotSupportedException("No CLUT allowed for direct pixel modes");

                    this.ReadCLUTs(reader);
                }

                this.ReadPixels(reader);
            }
        }

        private void ReadCLUTs(BinaryReader reader)
        {
            var clutBnum = reader.ReadUInt32(); // Data length of CLUT block. Units: bytes. Includes the 4 bytes of bnum

            _clutDataX = reader.ReadUInt16(); // x coordinate in frame buffer
            _clutDataY = reader.ReadUInt16(); // y coordinate in frame buffer

            _clutDataWidth = reader.ReadUInt16(); // Size of data in horizontal direction (CLUT entries)
            _clutDataHeight = reader.ReadUInt16(); // Size of data in vertical direction (CLUTs)

            //CLUT 1~n - CLUT entry (16 bits per entry)
            _clutData = new TimColor[_clutDataHeight][];
            for (int y = 0; y < _clutDataHeight; y++)
            {
                _clutData[y] = new TimColor[_clutDataWidth];
                for (int x = 0; x < _clutDataWidth; x++)
                    _clutData[y][x] = new TimColor(reader.ReadUInt16());
            }
        }

        private void ReadPixels(BinaryReader reader)
        {
            var pixelBnum = reader.ReadUInt32();  // Data length of pixel data. Units: bytes. Includes the 4 bytes of bnum

            _pixelDataX = reader.ReadUInt16(); // Frame buffer x coordinate
            _pixelDataY = reader.ReadUInt16(); // Frame buffer y coordinate

            _pixelDataWidth = reader.ReadUInt16(); // Size of data in vertical direction
            _pixelDataHeight = reader.ReadUInt16(); // Size of data in horizontal direction (in 16-bit units)

            //DATA 1~n - Frame buffer data (16 bits)
            _pixelData = new ushort[_pixelDataHeight][];
            for (int y = 0; y < _pixelDataHeight; y++)
            {
                _pixelData[y] = new ushort[_pixelDataWidth];
                for (int x = 0; x < _pixelDataWidth; x++)
                    _pixelData[y][x] = reader.ReadUInt16();
            }
        }

        public Bitmap ToBitmap()
        {
            var result = new Bitmap(this.Width, this.Height);

            var px0 = default(Color);
            var px1 = default(Color);
            var px2 = default(Color);
            var px3 = default(Color);

            //24 bit registers
            var w = 0;
            byte r0 = 0;
            byte r1 = 0;
            byte g0 = 0;
            byte g1 = 0;
            byte b0 = 0;
            byte b1 = 0;

            for (int y = 0; y < _pixelDataHeight; y++)
            {
                for (int x = 0; x < _pixelDataWidth; x++)
                {
                    var pixel = _pixelData[y][x];
                    switch (_flag.PixelMode)
                    {
                        case TimPixelMode.CLUT4:
                            px0 = _clutData[this.ClutIndex][pixel >> 0 & 0x000F].ToColor(this.Transparency);
                            px1 = _clutData[this.ClutIndex][pixel >> 4 & 0x000F].ToColor(this.Transparency);
                            px2 = _clutData[this.ClutIndex][pixel >> 8 & 0x000F].ToColor(this.Transparency);
                            px3 = _clutData[this.ClutIndex][pixel >> 12 & 0x000F].ToColor(this.Transparency);

                            result.SetPixel(x * 4 + 0, y, px0);
                            result.SetPixel(x * 4 + 1, y, px1);
                            result.SetPixel(x * 4 + 2, y, px2);
                            result.SetPixel(x * 4 + 3, y, px3);
                            break;

                        case TimPixelMode.CLUT8:
                            px0 = _clutData[this.ClutIndex][pixel >> 0 & 0x000F].ToColor(this.Transparency);
                            px1 = _clutData[this.ClutIndex][pixel >> 4 & 0x000F].ToColor(this.Transparency);

                            result.SetPixel(x * 2 + 0, y, px0);
                            result.SetPixel(x * 2 + 1, y, px1);
                            break;

                        case TimPixelMode.Direct15:
                            px0 = new TimColor(pixel).ToColor(this.Transparency);

                            result.SetPixel(x, y, px0);
                            break;

                        case TimPixelMode.Direct24:
                            if (w == 0)
                            {
                                r0 = Convert.ToByte(pixel >> 0 & 0x00FF);
                                g0 = Convert.ToByte(pixel >> 8 & 0x00FF);
                                w++;
                            }
                            else if (w == 1)
                            {
                                b0 = Convert.ToByte(pixel >> 0 & 0x00FF);
                                r1 = Convert.ToByte(pixel >> 8 & 0x00FF);
                                w++;
                            }
                            else if (w == 2)
                            {
                                g1 = Convert.ToByte(pixel >> 0 & 0x00FF);
                                b1 = Convert.ToByte(pixel >> 8 & 0x00FF);
                                w = 0;

                                px0 = Color.FromArgb(r0, g0, b0);
                                px1 = Color.FromArgb(r1, g1, b1);

                                result.SetPixel(x / 2 + 0, y, px0);
                                result.SetPixel(x / 2 + 1, y, px1);
                            }
                            break;

                        case TimPixelMode.Mixed:
                            throw new NotSupportedException("Mixed mode is not supported.");

                        default:
                            break;
                    }
                }
            }

            return result;
        }

#if UNSAFE

        public unsafe Bitmap ToBitmapUnsafe()
        {
            var result = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var px0 = default(Color);
            var px1 = default(Color);
            var px2 = default(Color);
            var px3 = default(Color);

            //24 bit registers
            var w = 0;
            byte r0 = 0;
            byte r1 = 0;
            byte g0 = 0;
            byte g1 = 0;
            byte b0 = 0;
            byte b1 = 0;

            var resultData = result.LockBits(new Rectangle(Point.Empty, result.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, result.PixelFormat);

            var pixelSize = Image.GetPixelFormatSize(result.PixelFormat) / 8;
            var pixelPtr = (byte*)resultData.Scan0;

            for (int y = 0; y < _pixelDataHeight; y++)
            {
                for (int x = 0; x < _pixelDataWidth; x++)
                {
                    var pixel = _pixelData[y][x];
                    switch (_flag.PixelMode)
                    {
                        case TimPixelMode.CLUT4:
                            px0 = _clutData[this.ClutIndex][pixel >> 0 & 0x000F].ToColor(this.Transparency);
                            px1 = _clutData[this.ClutIndex][pixel >> 4 & 0x000F].ToColor(this.Transparency);
                            px2 = _clutData[this.ClutIndex][pixel >> 8 & 0x000F].ToColor(this.Transparency);
                            px3 = _clutData[this.ClutIndex][pixel >> 12 & 0x000F].ToColor(this.Transparency);

                            this.SetPixelUnsafe(pixelPtr, px0);
                            pixelPtr += pixelSize;

                            this.SetPixelUnsafe(pixelPtr, px1);
                            pixelPtr += pixelSize;

                            this.SetPixelUnsafe(pixelPtr, px2);
                            pixelPtr += pixelSize;

                            this.SetPixelUnsafe(pixelPtr, px3);
                            pixelPtr += pixelSize;
                            break;

                        case TimPixelMode.CLUT8:
                            px0 = _clutData[this.ClutIndex][pixel >> 0 & 0x000F].ToColor(this.Transparency);
                            px1 = _clutData[this.ClutIndex][pixel >> 4 & 0x000F].ToColor(this.Transparency);

                            this.SetPixelUnsafe(pixelPtr, px0);
                            pixelPtr += pixelSize;

                            this.SetPixelUnsafe(pixelPtr, px1);
                            pixelPtr += pixelSize;
                            break;

                        case TimPixelMode.Direct15:
                            px0 = new TimColor(pixel).ToColor(this.Transparency);
                            this.SetPixelUnsafe(pixelPtr, px0);

                            break;

                        case TimPixelMode.Direct24:
                            if (w == 0)
                            {
                                r0 = Convert.ToByte(pixel >> 0 & 0x00FF);
                                g0 = Convert.ToByte(pixel >> 8 & 0x00FF);
                                w++;
                            }
                            else if (w == 1)
                            {
                                b0 = Convert.ToByte(pixel >> 0 & 0x00FF);
                                r1 = Convert.ToByte(pixel >> 8 & 0x00FF);
                                w++;
                            }
                            else if (w == 2)
                            {
                                g1 = Convert.ToByte(pixel >> 0 & 0x00FF);
                                b1 = Convert.ToByte(pixel >> 8 & 0x00FF);
                                w = 0;

                                px0 = Color.FromArgb(r0, g0, b0);
                                px1 = Color.FromArgb(r1, g1, b1);

                                this.SetPixelUnsafe(pixelPtr, px0);
                                pixelPtr += pixelSize;

                                this.SetPixelUnsafe(pixelPtr, px1);
                                pixelPtr += pixelSize;
                            }
                            break;

                        case TimPixelMode.Mixed:
                            throw new NotSupportedException("Mixed mode is not supported.");
                    }
                }
            }

            result.UnlockBits(resultData);
            return result;
        }

        private unsafe void SetPixelUnsafe(byte* pPixel, Color color)
        {
            //ARGB -> reverse order
            pPixel[0] = color.B;
            pPixel[1] = color.G;
            pPixel[2] = color.R;
            pPixel[3] = color.A;
        }

#endif

        #region Dispose

        private bool disposed;

        public bool Disposed

        {
            get { return disposed; }
            set { disposed = value; }
        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.

                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~Tim()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion Dispose
    }
}