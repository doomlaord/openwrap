﻿using OpenWrap.Commands;
using OpenWrap.Repositories;

namespace OpenWrap.PackageManagement
{
    public class PackageUpdatedResult : PackagePublishedResult
    {
        public PackageUpdatedResult(IPackageInfo previousPackage, IPackageInfo package, ISupportPublishing repository)
                : base(package, repository)
        {
            PreviousPackage = previousPackage;
        }

        public IPackageInfo PreviousPackage { get; set; }
        public override ICommandOutput ToOutput()
        {
            return new Info("{0}: {1} updated [{2} -> {3}].", Repository.Name, PreviousPackage.Name, PreviousPackage.Version, Package.Version);
        }
    }
}