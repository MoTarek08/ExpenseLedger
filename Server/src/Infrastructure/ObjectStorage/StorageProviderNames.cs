using Domain.Entities.DomainEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.ObjectStorage
{
    public static class StorageProviderNames
    {
        public const string R2 = "R2";
        public const string S3 = "S3";
        public const string MinIO = "MinIO";

        public static string From(StorageProvider provider) => provider switch
        {
            StorageProvider.R2 => R2,
            StorageProvider.S3 => S3,
            StorageProvider.MinIO => MinIO,
            _ => throw new ArgumentOutOfRangeException(nameof(provider),
                 $"No provider name mapped for {provider}.")
        };
    }
}
