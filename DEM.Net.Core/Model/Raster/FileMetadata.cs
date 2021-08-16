﻿// FileMetadata.cs
//
// Author:
//       Xavier Fischer 
//
// Copyright (c) 2019 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEM.Net.Core
{
    /// <summary>
    /// Metadata file generated on the fly.
    /// Extracts necessary data in order to index files for querying without actually open them (which is costly)
    /// </summary>
    [ProtoContract]
    public class FileMetadata : IEquatable<FileMetadata>
    {
        #region Versioning

        /* History
         * 
         *  2.1 : file name are relative to data directory
         *  2.2 : [Metadata regneration required] file format is now mapped to DEMFileDefinition, lat/lon bounds names changed for clarity, file format changed from DEMFileFormat (name + file extenstion)
         */
        [ProtoMember(1)]
        public const string FILEMETADATA_VERSION = "2.2";
        #endregion

        internal FileMetadata()
        {
        }

        public FileMetadata(string filename, DEMFileDefinition fileFormat, string version = FILEMETADATA_VERSION)
        {
            this.Filename = filename;
            this.FileFormat = fileFormat;
            this.Version = version;
        }

        /// <summary>
        /// Marks this metadata as virtual.
        /// Used for GetHeightMap when bbox does not cover DEM tiles.
        /// Virtual metadata data is then used to generate missing heightmaps
        /// </summary>
        [JsonIgnore]
        public bool VirtualMetadata { get; set; }

        [ProtoMember(2)]
        public string Version { get; set; }
        [ProtoMember(3)]
        public string Filename { get; set; }
        [ProtoMember(4)]
        public int Height { get; set; }
        [ProtoMember(5)]
        public int Width { get; set; }
        [ProtoMember(6)]
        public double PixelScaleX { get; set; }
        [ProtoMember(7)]
        public double PixelScaleY { get; set; }
        /// <summary>
        /// Data point start latitude (used for bbox)
        /// Image may be grid centered, with lat less than data start, but the data resides in the next overlapping tile
        /// </summary>
        [ProtoMember(8)]
        public double DataStartLat { get; set; }
        /// <summary>
        /// Data point start longitude (used for bbox)
        /// Image may be grid centered, with long less than data start, but the data resides in the next overlapping tile
        /// </summary>
        [ProtoMember(9)]
        public double DataStartLon { get; set; }
        /// <summary>
        /// Data point end latitude (used for bbox)
        /// </summary>
        [ProtoMember(10)] 
        public double DataEndLat { get; set; }
        /// <summary>
        /// Data point end longitude (used for bbox)
        /// </summary>
        [ProtoMember(11)] 
        public double DataEndLon { get; set; }
        [ProtoMember(12)]
        public int BitsPerSample { get; set; }
        [ProtoMember(13)]
        public string WorldUnits { get; set; }
        [ProtoMember(14)]
        public string SampleFormat { get; set; }
        [ProtoMember(15)]
        public string NoDataValue { get; set; }
        [ProtoMember(16)]
        public int ScanlineSize { get; set; }
        /// <summary>
        /// Origin longitude of physical image (for cell centered images this can be offset by 1px)
        /// </summary>
        [ProtoMember(17)]
        public double PhysicalStartLon { get; set; }
        ///
        /// Origin latitude of physical image (for cell centered images this can be offset by 1px)
        [ProtoMember(18)]
        public double PhysicalStartLat { get; set; }
        [ProtoMember(19)]
        public double PhysicalEndLon { get; set; }
        [ProtoMember(20)]
        public double PhysicalEndLat { get; set; }
        [ProtoMember(21)]
        public double pixelSizeX { get; set; }
        [ProtoMember(22)]
        public double pixelSizeY { get; set; }
        [ProtoMember(23)]
        public DEMFileDefinition FileFormat { get; set; }
        [ProtoMember(24)]
        public float MinimumAltitude { get; set; }
        [ProtoMember(25)]
        public float MaximumAltitude { get; set; }


        private float _noDataValue;
        private bool _noDataValueSet = false;

        public float NoDataValueFloat
        {
            get
            {
                if (!_noDataValueSet)
                {
                    _noDataValue = float.Parse(NoDataValue);
                    _noDataValueSet = true;
                }
                return _noDataValue;
            }
            set { _noDataValue = value; }
        }


        public override string ToString()
        {
            return $"{System.IO.Path.GetFileName(Filename)}: {BoundingBox}";
        }

        public override bool Equals(object obj)
        {
            FileMetadata objTyped = obj as FileMetadata;
            if (objTyped == null)
                return false;

            return this.Equals(objTyped);
        }
        public override int GetHashCode()
        {
            return Path.GetFileName(Filename).GetHashCode();
        }

        public bool Equals(FileMetadata other)
        {
            if (this == null || other == null)
                return false;
            return this.GetHashCode().Equals(other.GetHashCode());
        }

        private BoundingBox _boundingBox;
        [JsonIgnore]
        public BoundingBox BoundingBox
        {
            get
            {
                if (_boundingBox == null)
                {
                    _boundingBox = new BoundingBox(
                                Math.Min(DataStartLon, DataEndLon),
                                Math.Max(DataStartLon, DataEndLon),
                                Math.Min(DataStartLat, DataEndLat),
                                Math.Max(DataStartLat, DataEndLat));
                }
                return _boundingBox;
            }
        }

        public FileMetadata Clone()
        {
            FileMetadata clone = (FileMetadata)this.MemberwiseClone();
            clone.Filename = Guid.NewGuid().ToString();
            clone._boundingBox = null;
            return clone;
        }

    }
}
