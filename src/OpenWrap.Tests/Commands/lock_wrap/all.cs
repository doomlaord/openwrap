using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.lock_wrap
{
    public class all : contexts.lock_wrap
    {
        public all()
        {
            given_project_repository(new InMemoryRepository { CanLock = true });
            given_project_package("one-ring", "2.0.1.3");
            given_dependency("depends: one-ring");

            when_executing_command();
        }

        [Test]
        public void command_succeeds()
        {
            Results.ShouldHaveNoError();
        }
        [Test]
        public void package_is_locked()
        {
            project_repo.ShouldHaveLock("one-ring", "2.0.1.3");
        }

        [Test]
        public void package_lock_is_output()
        {
            Results.ShouldHaveOne<PackageLocked>().Package.Version.ShouldBe("2.0.1.3".ToVersion());

        }
    }
}