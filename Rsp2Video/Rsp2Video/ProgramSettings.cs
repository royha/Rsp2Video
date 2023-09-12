using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    public class ProgramSettings
    {
        /// <summary>
        /// Stores the file path on panel1.
        /// </summary>
        public string LastUsedFile { get; set; }

        /// <summary>
        /// When true, starts the output filename with "Reverse Speech of ".
        /// When false, starts the output filename with "Backward Voice Analysis of ".
        /// </summary>
        public Boolean ReverseSpeechTrademarkAuthorized { get; set; }

        public ProgramSettings()
        {
            this.ReverseSpeechTrademarkAuthorized = false;
        }
    }
}
