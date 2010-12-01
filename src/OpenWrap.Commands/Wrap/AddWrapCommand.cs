﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;
using OpenWrap.PackageManagement;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "add", Noun = "wrap")]
    public class AddWrapCommand : WrapCommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool NoDescriptorUpdate { get; set; }

        bool? _project;

        bool? _system;

        [CommandInput]
        public bool Project
        {
            get { return _project ?? (_system == null); }
            set { _project = value; }
        }

        [CommandInput]
        public bool System
        {
            get { return _system ?? _project == null; }
            set { _system = value; }
        }

        [CommandInput]
        public bool Content { get; set; }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        [CommandInput]
        public string MinVersion { get; set; }

        [CommandInput]
        public string MaxVersion { get; set; }

        [CommandInput]
        public bool Anchored { get; set; }


        bool ShouldUpdateDescriptor
        {
            get
            {
                return NoDescriptorUpdate == false &&
                       Environment.Descriptor != null;
            }
        }

        public AddWrapCommand()
        {
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(ValidateInputs()).Or(ExecuteCore());
        }

        IEnumerable<ICommandOutput> ValidateInputs()
        {
            var gotVersion = Version != null;
            var gotMinVersion = MinVersion != null;
            var gotMaxVersion = MaxVersion != null;
            var numberOfVersionInputTypes = (new[] { gotVersion, (gotMinVersion || gotMaxVersion) }).Count(v => v);

            if (numberOfVersionInputTypes > 1)
            {
                yield return new Error("Arguments for 'version' and 'version boundaries' cannot be combined.");
                yield break;
            }

            if (gotVersion && Version.ToVersion() == null)
            {
                yield return new Error("Could not parse version: " + Version);
                yield break;
            }

            if (gotMinVersion && MinVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse minversion: " + MinVersion);
                yield break;
            }

            if (gotMaxVersion && MaxVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse maxversion: " + MaxVersion);
                yield break;
            }

            if (Project && Environment.ProjectRepository == null)
            {
                yield return new Error("Project repository doesn't exist but -project has been specified.");
                yield break;

            }
        }

        IEnumerable<ICommandOutput> ExecuteCore()
        {
            if (Name.EndsWith(".wrap", StringComparison.OrdinalIgnoreCase))
            {
                yield return WrapFileToPackageDescriptor();
            }

            yield return VerifyWrapFile();
            yield return VeryfyWrapRepository();

            var sourceRepositories = new[] { Environment.CurrentDirectoryRepository, Environment.SystemRepository }.Concat(Environment.RemoteRepositories);

            if (Project && System)
            {
                var sysToAdd = new List<PackageIdentifier>();
                foreach (var m in PackageManager.AddProjectPackage(this.PackageRequest, sourceRepositories, Environment.Descriptor, Environment.ProjectRepository, AddOptions))
                {
                    yield return ToOutput(m);
                    var added = m as PackagePublishedResult;
                    if (added != null)
                        sysToAdd.Add(added.Package.Identifier);
                }
                foreach (var identifier in sysToAdd)
                foreach (var m in PackageManager.AddSystemPackage(PackageRequest.Exact(identifier.Name, identifier.Version), sourceRepositories, Environment.SystemRepository))
                        yield return ToOutput(m);
            }
            else if (Project)
            {
                foreach (var m in PackageManager.AddProjectPackage(this.PackageRequest, sourceRepositories, Environment.Descriptor, Environment.ProjectRepository, AddOptions))
                    yield return ToOutput(m);
            }
            else if (System)
            {
                foreach (var m in PackageManager.AddSystemPackage(PackageRequest, sourceRepositories, Environment.SystemRepository, AddOptions))
                    yield return ToOutput(m);
            }


            if (ShouldUpdateDescriptor)
                TrySaveDescriptorFile();
        }

        PackageAddOptions AddOptions
        {
            get {
                PackageAddOptions addOptions = 0;
                if (this.Anchored)
                    addOptions |= PackageAddOptions.Anchor;
                if (Content)
                    addOptions |= PackageAddOptions.Content;
                if (!NoDescriptorUpdate)
                    addOptions |= PackageAddOptions.UpdateDescriptor;
                return addOptions;
            }
        }

        protected PackageRequest PackageRequest
        {
            get
            {
                if (Version != null) return PackageRequest.Exact(Name, Version.ToVersion());
                if (MinVersion != null || MaxVersion != null) return PackageRequest.Between(Name, MinVersion.ToVersion(), MaxVersion.ToVersion());
                return PackageRequest.Any(Name);
            }
        }


        ICommandOutput WrapFileToPackageDescriptor()
        {
            if (Path.GetExtension(Name).Equals(".wrap", StringComparison.OrdinalIgnoreCase) && Environment.CurrentDirectory.GetFile(Path.GetFileName(Name)).Exists)
            {
                var originalName = Name;
                Name = PackageNameUtility.GetName(Path.GetFileNameWithoutExtension(Name));
                Version = PackageNameUtility.GetVersion(Path.GetFileNameWithoutExtension(originalName)).ToString();
                return
                        new GenericMessage(
                                string.Format("The requested package contained '.wrap' in the name. Assuming you pointed to the file in the current directory and meant a package named '{0}' with version qualifier '{1}'.",
                                              Name,
                                              Version))
                        {
                            Type = CommandResultType.Warning
                        };
            }
            if (File.Exists(Name))
            {
                return new Error("You have given a path to a .wrap file that is not in the current directory but exists on disk. This is not currently supported. Go to the directory, and re-issue the command.");
            }
            return null;
        }


        ICommandOutput VerifyWrapFile()
        {
            if (NoDescriptorUpdate)
                return new GenericMessage("Wrap descriptor ignored.");
            return Environment.Descriptor == null
                       ? new GenericMessage(@"No wrap descriptor found, installing locally.")
                       : new GenericMessage("Wrap descriptor found.");
        }

        ICommandOutput VeryfyWrapRepository()
        {
            return Environment.ProjectRepository != null
                       ? new GenericMessage("Project repository present.")
                       : new GenericMessage("Project repository absent.");
        }
    }
}