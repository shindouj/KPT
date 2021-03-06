﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPT
{
    static class DebugSettings
    {
        public const bool SKIP_ORIGINAL_DIRECTORY_COPYING = false;
        public const bool IGNORE_DIRECTORY_ALREADY_EXISTS_WARNING = true;
        public const bool ALLOW_FILE_WRITES = true;
        public const bool COPY_UNPACKED_FILES = true;
        public const bool REFRESH_REPACKED_FILES_ON_BUILD = true;
        public const bool ATTEMPT_DECOMPRESSION = true;
        public const bool USE_BACKGROUND_WORKERS = true;

    }
}
