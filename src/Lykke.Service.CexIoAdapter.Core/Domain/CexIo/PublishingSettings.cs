﻿using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public sealed class PublishingSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string Exchanger { get; set; }
        public bool Enabled { get; set; }
    }
}
