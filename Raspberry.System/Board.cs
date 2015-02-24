#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

#endregion

namespace Raspberry
{
    /// <summary>
    /// Represents the Raspberry Pi mainboard.
    /// </summary>
    /// <remarks>Version and revisions are based on <see cref="http://raspberryalphaomega.org.uk/2013/02/06/automatic-raspberry-pi-board-revision-detection-model-a-b1-and-b2/"/>.</remarks>
    public class Board
    {
        #region Fields

        private static readonly Lazy<Board> board = new Lazy<Board>(LoadBoard);
        private readonly Dictionary<string, string> settings;

        private string[] raspberryPiProcessor = new string[] { "BCM2708", "BCM2709" };
        
        #endregion

        #region Instance Management

        private Board(Dictionary<string, string> settings)
        {
            this.settings = settings;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current mainboard configuration.
        /// </summary>
        public static Board Current
        {
            get { return board.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a Raspberry Pi.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a Raspberry Pi; otherwise, <c>false</c>.
        /// </value>
        public bool IsRaspberryPi
        {
            get { return (Array.IndexOf(raspberryPiProcessor, Processor) >= 0); }
        }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        public string Processor
        {
            get
            {
                string hardware;
                return settings.TryGetValue("Hardware", out hardware) ? hardware : null;
            }
        }

        /// <summary>
        /// Gets the board firmware version.
        /// </summary>
        public int Firmware
        {
            get
            {
                string revision;
                int firmware;
                if (settings.TryGetValue("Revision", out revision) && !string.IsNullOrEmpty(revision) && int.TryParse(revision, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out firmware))
                    return firmware;

                return 0;
            }
        }

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public string SerialNumber
        {
            get { 
                string serial;
                if (settings.TryGetValue("Serial", out serial) && !string.IsNullOrEmpty(serial))
                    return serial;

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Raspberry Pi board is overclocked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Raspberry Pi is overclocked; otherwise, <c>false</c>.
        /// </value>
        public bool IsOverclocked
        {
            get
            {
                var firmware = Firmware;
                return (firmware & 0xFFFF0000) != 0;
            }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns>The model name (<c>A</c> or <c>B</c>) if known; otherwise, <c>(char)0</c>.</returns>
        public char Model
        {
            get
            {
                var firmware = Firmware;
                switch(firmware & 0xFFFF)
                {
                    case 0x7:
                    case 0x8:
                    case 0x9:
                        return 'A';

                    case 0x2:
                    case 0x3:
                    case 0x4:
                    case 0x5:
                    case 0x6:
                    case 0xd:
                    case 0xe:
                    case 0xf:
                    case 0x10:
                        return 'B';

                    case 0x1040:
                    case 0x1041:
                        return '2';

                    default:
                        return (char)0;
                }
            }
        }

        /// <summary>
        /// Gets the board revision.
        /// </summary>
        /// <returns>The board revision for the given <see cref="Model"/> if known; otherwise, <c>0</c>.</returns>
        public int Revision
        {
            get
            {
                var firmware = Firmware;
                switch (firmware & 0xFFFF)
                {
                    case 0x7:
                    case 0x8:
                    case 0x9:
                        return 1;   // Model A, rev1

                    case 0x2:
                    case 0x3:
                        return 1;   // Model B, rev1

                    case 0x4:
                    case 0x5:
                    case 0x6:
                    case 0xd:
                    case 0xe:
                    case 0xf:
                        return 2;   // Model B, rev2

                    case 0x10:
                        return 3;   // Model B+, rev3
                    
                    case 0x1040:
                    case 0x1041:
                        return 4;
 
                    default:
                        return 0;   // Unknown
                }
            }
        }

        #endregion

        #region Private Helpers

        private static Board LoadBoard()
        {
            try
            {
                const string filePath = "/proc/cpuinfo";
                string[] cpuInfo = File.ReadAllLines(filePath);
                Dictionary<string, string> settings = new Dictionary<string, string>();
                string suffix = "";
                foreach(var l in cpuInfo)
                {
                    var separator = l.IndexOf(':');
                    string key = l;
                    string val = null;
                    if (!string.IsNullOrWhiteSpace(l) && separator > 0)
                    {
                        key = l.Substring(0, separator).Trim();
                        val = l.Substring(separator + 1).Trim();
                        if (String.Compare(key, "processor", true) == 0)
                        {
                            suffix = "." + val;
                        }
                        settings.Add(key + suffix, val);
                    }
                    else
                    {
                        suffix = "";
                    }
                }
                return new Board(settings);
            }
            catch
            {
                return new Board(new Dictionary<string, string>());
            }
        }

        #endregion
    }
}