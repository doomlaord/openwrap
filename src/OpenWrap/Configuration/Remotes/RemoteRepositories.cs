﻿using System;
using OpenWrap.Collections;

namespace OpenWrap.Configuration.Remotes
{
    [Path("remotes")]
    public class RemoteRepositories : IndexedDictionary<string, RemoteRepository>
    {
        public static readonly RemoteRepositories Default =
            new RemoteRepositories
            {
                {
                    "openwrap", new RemoteRepository
                    {
                        FetchRepository = new RemoteRepositoryEndpoint
                        {
                            Token = "[indexed-http]http://wraps.openwrap.org/index.wraplist"
                        }
                    }
                    }
            };

        public RemoteRepositories()
            : base(_ => _.Name, (key, _) => _.Name = key, StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}