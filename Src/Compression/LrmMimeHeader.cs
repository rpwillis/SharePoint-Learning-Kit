/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Text.RegularExpressions;

namespace Microsoft.LearningComponents
{
    class LrmMimeHeader
    {
        public string ContentType;
        public string BoundaryString;
        public string MimeVersion;
        public string LrmVersion;
        public string MultiLR;

        public void ParseMimeHeader(string line)
        {
            line = line.Trim();
            if (line.StartsWith("Content-Type"))
            {
                ContentType = line.Substring(13, line.IndexOf(';', 13) - 13).Trim();
                Regex r = new Regex("boundary\\s*=\\s*\"(.*)\"", RegexOptions.IgnoreCase);
                GroupCollection matchGroups = r.Match(line).Groups;

                if(matchGroups.Count < 2)
                    throw new CompressionException(Properties.CompressionResources.LRMCorruptFile);

                BoundaryString = matchGroups[1].Value;

                if (String.IsNullOrEmpty(BoundaryString) || !ContentType.Contains("related"))
                    throw new CompressionException(Properties.CompressionResources.LRMCorruptFile);

            }
            else if (line.StartsWith("MIME-Version"))
            {
                MimeVersion = line.Substring(13).Trim();

                //Safe to ignore errors related to MIME version numbers
            }
            else if (line.StartsWith("X-LRM-Version"))
            {
                LrmVersion = line.Substring(14).Trim();

                if (LrmVersion.StartsWith("1.") == false)
                    throw new CompressionException(Properties.CompressionResources.LRMBadVersion);

            }
            else if (line.StartsWith("X-Multi-LR"))
            {
                MultiLR = line.Substring(11).Trim();

                //Is a X-Multi-LR with a value '0' or '1' allowed?

                throw new CompressionException(Properties.CompressionResources.LRMMultiLRNotSupported);
            }
            else
            {
                // Ignore an unrecognised error without error-ing
            }                        
        }
    }
}
