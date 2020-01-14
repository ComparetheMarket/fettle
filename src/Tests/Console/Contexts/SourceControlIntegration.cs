using System;
using Fettle.Core;
using Moq;

namespace Fettle.Tests.Console.Contexts
{
    class SourceControlIntegration : Default
    {
        protected void Given_locally_modified_files(params string[] localModifications)
        {
            MockSourceControlIntegration.Setup(x => x.FindLocallyModifiedFiles(It.IsAny<Config>()))
                                        .Returns(localModifications);
        }

        protected void Given_locally_modified_files_will_throw_an_exception(Exception exception)
        {
            MockSourceControlIntegration.Setup(x => x.FindLocallyModifiedFiles(It.IsAny<Config>()))
                                        .Throws(exception);
        }
    }
}
