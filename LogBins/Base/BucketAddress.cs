using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct BucketAddress : IEquatable<BucketAddress>
    {
        public ushort TrainId { get; set; }
        public ushort BagId { get; set; }
        public int BucketId { get; set; }

        public bool Equals(BucketAddress other)
        {
            return TrainId == other.TrainId
                && BagId == other.BagId
                && BucketId == other.BucketId;
        }

        public override bool Equals(object obj)
        {
            if (obj is BucketAddress == false)
                return false;

            return Equals((BucketAddress)obj);
        }

        public override int GetHashCode()
        {
            return TrainId.GetHashCode()
                + BagId.GetHashCode()
                + BucketId.GetHashCode();                
        }

        public override string ToString()
        {
            return $"{TrainId}/{BagId}/{BucketId}";
        }

        public static bool operator ==(BucketAddress left, BucketAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BucketAddress left, BucketAddress right)
        {
            return !(left == right);
        }
    }
}
